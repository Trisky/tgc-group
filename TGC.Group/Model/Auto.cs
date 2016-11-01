﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.Camara;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.GrupoMs.Camara;
using Microsoft.DirectX.DirectInput;
using TGC.Group.Model;
using TGC.Core.Utils;
using TGC.Core.BoundingVolumes;
using TGC.Core.Geometry;
using TGC.Core.Collision;
using System.Drawing;
using TGC.GroupoMs.Model.efectos;
using TGC.Core.Shaders;
using Microsoft.DirectX.Direct3D;
//using TGC.Core.SceneLoader;

namespace TGC.GroupoMs.Model
{
    public class Auto : MovingObject
    {


        private List<TgcMesh> MeshesCercanos;
        public bool pintarObb;
        //public bool collisionResult;
        public float obbPosY = 0;
        public bool EsAutoJugador { get; set; }
        private Vector3 scale3;

        public List<Arma> Armas { get; set; }
        public Arma ArmaSeleccionada { get; set; }


        public TgcMesh RuedaMainMesh { get; set; }
        //public int MyProperty { get; set; }

        public Ruedas RuedasTraseras { get; set; }
        public Ruedas RuedasDelanteras { get; set; }
        public LucesAuto Luces { get; set; }
        public bool RenderLuces { get; set; }
        public string TechniqueOriginal { get; private set; }



        //----------- Boundig box --------------
        public TgcBoundingOrientedBox obb;
        public TgcScene ciudadScene { get; set; }
        public TgcScene bosqueScene { get; set; }
        private bool nitroActivado;
        public bool finishedLoading;
        public bool volanteo;
        private Microsoft.DirectX.Direct3D.Effect efectoShaderNitroHummer;
        private Microsoft.DirectX.Direct3D.Effect efectoOriginal;

        //--



        public Auto(string nombre, float vida, float avanceMaximo, float reversaMax,
                    float aceleracion, float desaceleracion,
                    TgcMesh mesh, GameModel model, Ruedas ruedasAdelante, Ruedas ruedasAtras, TgcMesh ruedaMainMesh)
        {
            MeshesCercanos = new List<TgcMesh>();
            var scale = 0.6f;
            scale3 = new Vector3(scale, scale, scale);

            CrearHumoCanioDeEscape(model);
            AvanceMax = avanceMaximo;
            ReversaMax = -reversaMax;
            Desaceleracion = desaceleracion;
            InerciaNegativa = 1f;
            DireccionRuedas = 0f;
            Armas = new List<Arma>();
            aceleracionVertical = 0f;
            Velocidad = 0f;
            Mesh = mesh;
            Aceleracion = aceleracion;
            GameModel = model;

            RuedasTraseras = ruedasAtras;
            RuedasDelanteras = ruedasAdelante;
            RuedaMainMesh = ruedaMainMesh;

            //------------Ariel---------------
            Mesh.AutoTransformEnable = false;
            Mesh.AutoUpdateBoundingBox = false;

            obb = TgcBoundingOrientedBox.computeFromAABB(Mesh.BoundingBox);
            var yMin = Mesh.BoundingBox.PMin.Y;
            var yMax = Mesh.BoundingBox.PMax.Y;
            obbPosY = (yMax + yMin) / 2 + yMin;
            obb.Extents = new Vector3(obb.Extents.X, obb.Extents.Y, obb.Extents.Z * -1);
            ciudadScene = model.MapScene;
            bosqueScene = model.BosqueScene;
            

            //--------luces
            Luces = new LucesAuto(this, ruedasAdelante, ruedasAtras, CamaraAuto);
            RenderLuces = false;

            EsAutoJugador = true;
            fixEjecutado = false; //para arreglar el temita de que el auto no aparece. 

            //shader pal hummer
            TechniqueOriginal = Mesh.Technique;
            efectoOriginal = Mesh.Effect;
            efectoShaderNitroHummer = TgcShaders.loadEffect(GameModel.ShadersDir + "ShaderHummer.fx");
        }


        [Obsolete]
        public void recibirDanio(int cant)
        {
            if (this.VidaMaxima < 0)
            {
                return;
            }
            this.Vida = -cant;
            if (this.Vida <= 0)
            {
                //TODO this.morir; //explota?
            }
            if (this.EsAutoJugador)
            {
                /*TODO mostrar en pantalla danio recibido,con 200ms seria suficiente, 
                podria ir sumandose para saber cuanto le sacamos en esa "rafaga" de disparos.*/
            }
            else
            {
                //TODO mostrar en pantalla danio dado
            }
        }

        [Obsolete]
        public void Chocar(Vector3 angulo)
        {
            Velocidad = -Velocidad * 0.2f;
            PosicionRollback();
        }

        /// <summary>
        /// Llamado una vez por frame
        /// </summary>
        /// <param name="input"></param>
        public void Update(TgcD3dInput input)
        {
            nitroActivado = false;
            volanteo = false;
            BugFixAutoNoAparece();

            huboMovimiento = huboRotacion = false;
            bool collisionFound = huboSalto = false;
            //pintarObb = false;

            PosicionAnterior = Mesh.Position;
            RotacionAnterior = Mesh.Rotation;

            //1 acelerar
            if (input.keyDown(Key.W))
            {
                Acelero(1);
                huboMovimiento = true;

                if (input.keyDown(Key.LeftShift))
                {
                    Acelero(0.5f); //turbo!
                    nitroActivado = true;
                }
            }

            //2 frenar, reversa
            if (input.keyDown(Key.S))
            {
                Freno();
                huboMovimiento = true;
            }

            //3 izquierda TODO//girar ruedas
            if (input.keyDown(Key.A))
            {
                DoblarRuedas(1);
            }

            //4 derecha TODO //girar ruedas

            if (input.keyDown(Key.D))
            {
                DoblarRuedas(-1);
            }

            if (input.keyDown(Key.F))
            {
                pintarObb = !pintarObb;
            }

            //5 disparar TODO -- 
            if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_LEFT))
            {
                //disparar 
                //this.ArmaSeleccionada.ammo--;
                //TODO falta evento de fin de ammo.
            }

            //6 cambiar de arma TODO
            if (input.buttonUp(TgcD3dInput.MouseButtons.BUTTON_RIGHT))
            {

            }

            //7 freno de mano? maybe TODO
            if (input.keyPressed(Microsoft.DirectX.DirectInput.Key.Space))
            {

            }
            //8 saltar
            if (input.keyDown(Key.Space) && (Mesh.Position.Y == 5))
            {
                huboSalto = true;
            }

            else
            {
                //aceleracionVertical = -1; descomentar cuando el auto choque con el suelo.
            }

            //RuedasDelanteras.Update(Mesh.Position,Velocidad,DireccionRuedas);
            //RuedasTraseras.Update(Mesh.Position, Velocidad, DireccionRuedas);
            ProcesarInercia();
            MoverMesh();

            if (RenderLuces) Luces.Update();
            if (motionBlur != null) motionBlur.Update(0f);
            if (RenderLuces) Luces.Update();
            SeleccionarMeshesCercanos();
        }
        public void BugFixAutoNoAparece()
        {
            if (fixEjecutado) return;
            DireccionRuedas = 0.1f;
            Doblar();
            DireccionRuedas = 0f;
            fixEjecutado = true;
            GameModel.FinishedLoading = true; //para render de niebla
        }
        private void DoblarRuedas(int lado)
        {
            volanteo = true;
            DireccionRuedas = +lado * 0.4f;
            Doblar();
        }
        private void Doblar()
        {
            float lado = DireccionRuedas;
            lado = lado * Velocidad;
            angOrientacionMesh += lado * 1f * GameModel.ElapsedTime;
            //ahora ya tengo para el lado en el que voy a girar y la intensidad del giro, entonces giro el auto.

            CamaraAuto.rotateY(-lado * 1f * GameModel.ElapsedTime);
            anguloFinal = anguloFinal - lado * 1f * GameModel.ElapsedTime;
            matrixRotacion = Matrix.RotationY(anguloFinal);
            obb.rotate(new Vector3(0, -lado * 1f * GameModel.ElapsedTime, 0));
        }

        private void MoverMesh()
        {
            newPosicion = new Vector3(Mesh.Position.X + calcularDX(), calcularDY(), Mesh.Position.Z + calcularDZ());

            //4- las ruedas
            //RuedasDelanteras.Update(Mesh.Position, Velocidad, DireccionRuedas);
            //RuedasTraseras.Update(Mesh.Position, Velocidad, DireccionRuedas);

            //??
            obb.Center = new Vector3(Mesh.Position.X + calcularDX(), obbPosY + 0 + calcularDY(), Mesh.Position.Z + calcularDZ());

            //6 - me muevo
            var m = matrixRotacion *
                Matrix.Translation(newPosicion);

            Mesh.Transform = m; //Mesh.BoundingBox.transform(m);

            //RuedasDelanteras.Update3(Mesh.Position, matrixRotacion, angOrientacionMesh, Velocidad);
            //RuedasTraseras.Update3(Mesh.Position, matrixRotacion, angOrientacionMesh, Velocidad);
            var auxDireccion = DireccionRuedas;
            if (!volanteo)
                auxDireccion = 0;
            RuedasDelanteras.Update4(m, Velocidad, -auxDireccion);
            RuedasTraseras.Update4(m, Velocidad, 0);

            Mesh.Position = newPosicion;
            humoEscape.Update(newPosicion, Mesh.Rotation);

            //camera update
            if (CamaraAuto != null) //actualizo la posicion de la camara respecto de la del mesh
                CamaraAuto.Target = Mesh.Position;


            //7 ---- colisiones---
            ProcesarColisiones();
        }
        
        private void ProcesarColisiones()
        {
            bool collisionFound = false;
            foreach (var sceneMesh in MeshesCercanos)
            {
                var escenaAABB = sceneMesh.BoundingBox;
                var collisionResult = TgcCollisionUtils.testObbAABB(obb, escenaAABB);

                //2 -si lo hizo, salgo del foreach.
                if (collisionResult)
                {
                    collisionFound = true;
                    break;
                }
            }
            //3 - si chocó, pongo el bounding box en rojo (apretar F para ver el bb).
            if (collisionFound)
            {
                obb.setRenderColor(Color.Red);
                PosicionRollback();
            }
            else
            {
                obb.setRenderColor(Color.Yellow);
            }
        }

        /// <summary>
        /// rendereo.
        /// </summary>
        public void Render()
        {
            AplicarShader(); //para que cambie de color al meter nitro
            Mesh.render();
            RuedasDelanteras.Render();
            RuedasTraseras.Render();
            //RuedaMainMesh.render();
            if (RenderLuces)
                Luces.Update();
            if (motionBlur != null && finishedLoading) motionBlur.Render();

            //escenario.BoundingBox.render();

            if (pintarObb)
                obb.render();

            foreach (var mesh in ciudadScene.Meshes)
            {
                mesh.BoundingBox.render();
            }
            humoEscape.Render(nitroActivado);
    }
        
        private void AplicarShader()
        {
            if (!EsAutoJugador) return;
            efectoShaderNitroHummer.SetValue("time", GameModel.ElapsedTime);
            efectoShaderNitroHummer.SetValue("Velocidad", 4*Velocidad);
            if (Velocidad<2.5f )
            {
                Mesh.Effect = efectoOriginal;
                Mesh.Technique = TechniqueOriginal;
            }
            else
            {
                Mesh.Effect = efectoShaderNitroHummer;
                Mesh.Technique = "RenderScene";
            }
        }

        //OPTIMIZACION
        private void SeleccionarMeshesCercanos()
        {
            //para la ciudad
            foreach (var sceneMesh in ciudadScene.Meshes)
            {
                if (MeshEstaCerca(sceneMesh))
                    MeshesCercanos.Add(sceneMesh);
            }
            //para el bosque
            foreach (var sceneMesh in bosqueScene.Meshes)
            {
                if (MeshEstaCerca(sceneMesh))
                    MeshesCercanos.Add(sceneMesh);
            }

        }
        float distanciaDefault = 100f; //los meshes que estan mas lejos que esto no los evaluo en las colisiones.
        private bool MeshEstaCerca(TgcMesh sceneMesh)
        {
            float d = TgcCollisionUtils.sqDistPointAABB(Mesh.Position, sceneMesh.BoundingBox);

            if (d < distanciaDefault) return true;
            else return false;
        }
        //FIN OPTIMIZACION
    }
}
