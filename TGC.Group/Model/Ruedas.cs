﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Example;
using TGC.Core.Text;
using TGC.Core.Utils;

namespace TGC.GroupoMs.Model
{
    /// <summary>
    /// Representa un par de ruedas: delanteras (que giran) o traseras (que no)
    /// </summary>
    public class Ruedas
    {
        public float aux = 0;
        public float angRotRueda = 0;
        public float radioRueda = 5f;
        private float angulo;

        public TgcMesh RuedaMeshIzq { get; set; }
        public TgcMesh RuedaMeshDer { get; set; }
        public Vector3 OffsetRuedaDer { get; set; }
        public Vector3 OffsetRuedaIzq { get; set; }
        public Vector3 Separacion { get; set; }
        public bool SonDelanteras { get; set; } //para saber si las tenemos que rotar cuando giro el volante.
        public Vector3 Escala { get; set; }
        public Matrix MatrizPosInicialIzquierda { get; set; }
        public Matrix MatrizPosInicialDerecha { get; set; }
        public Matrix MatrizMovimiento { get; set; }
        private Vector3 scale3;

        public Ruedas(TgcMesh ruedaMainMesh, Vector3 offsetRuedaIzq, Vector3 offsetRuedaDer, bool sonDelanteras, float scale)
        {
            scale3 = new Vector3(scale, scale, scale);
            OffsetRuedaDer = offsetRuedaDer;
            OffsetRuedaIzq = offsetRuedaIzq;
            //Escala = scale3;
            Escala = new Vector3(2f, 2f, 2f);
            angulo = 0;
            //Escala = ruedaMainMesh.Scale;
            SonDelanteras = sonDelanteras;

            //------------------- RUEDA IZQUIERDA --------------------------
            RuedaMeshIzq = ruedaMainMesh.createMeshInstance
                                (ruedaMainMesh.Name + offsetRuedaIzq.ToString() + "_Izq");
            RuedaMeshIzq.Scale = scale3;
            RuedaMeshIzq.AutoTransformEnable = false;
            MatrizPosInicialIzquierda = Matrix.Translation(OffsetRuedaIzq);
            //------------------- RUEDA DERECHA   --------------------------
            RuedaMeshDer = ruedaMainMesh.createMeshInstance
                                (ruedaMainMesh.Name + offsetRuedaDer.ToString() + "_Der");
            RuedaMeshDer.Scale = scale3;
            RuedaMeshDer.AutoTransformEnable = false;
            MatrizPosInicialDerecha = Matrix.Translation(offsetRuedaDer);
        }

        /// <summary>
        /// Recibe la posicion del mesh del auto, 
        /// la velocidad del mismo para calcular rotacion y posicion del volante
        /// </summary>
        /// <param name="posicion"></param>
        /// <param name="velocidad"></param>
        [Obsolete]
        public void Update(Vector3 posicionAuto, float velocidad, float DireccionRuedas)
        {
            //1- ubico las ruedas en su posicion
            //RuedaMeshDer.Position = posicionAuto + OffsetRuedaDer;
            //RuedaMeshIzq.Position = posicionAuto + OffsetRuedaIzq;

            if (SonDelanteras)
            {
                //TODO girar las ruedas segun posicion del volante
            }

            //TODO rotar las ruedas segun velocidad del auto
            //RotarRuedas(velocidad,Escala);
            //TODO
        }
        [Obsolete]
        public void Update2(Matrix MR, Matrix MT, Vector3 Mpos)
        {
            //Matrix.RotationZ((float)Math.PI * 270 / 180) *
            if (SonDelanteras)
            {
                RuedaMeshIzq.Transform = Matrix.Scaling(Escala)
                    * MT * Matrix.Translation(OffsetRuedaDer);
                RuedaMeshDer.Transform = Matrix.Scaling(Escala) * MT * Matrix.Translation(OffsetRuedaIzq);
            }
            else
            {
                RuedaMeshIzq.Transform = Matrix.Scaling(Escala)
                   * MT * Matrix.Translation(OffsetRuedaDer);
                RuedaMeshDer.Transform = Matrix.Scaling(Escala) * MT * Matrix.Translation(OffsetRuedaIzq);
            }
        }

        public void Update4(Matrix MR, float velocidad, float direccionRuedas)
        {
            //direccionRuedas = direccionRuedas*4;
            angulo -= FastMath.QUARTER_PI * 0.4f * velocidad;
            var velRotacion = Matrix.RotationX(angulo);
            RuedaMeshDer.Transform = Matrix.Scaling(Escala) * velRotacion * Matrix.RotationY(direccionRuedas) * MatrizPosInicialDerecha * MR;
            RuedaMeshIzq.Transform = Matrix.Scaling(Escala) * velRotacion * Matrix.RotationY(direccionRuedas) * MatrizPosInicialIzquierda * MR;
        }


        [Obsolete]
        public void Update3(Vector3 cochePos, Matrix MR, float angOrientacion, float v)
        {
            var aux2 = 1;
            if (SonDelanteras)
            {
                aux = (float)Math.PI;
            }
            else
            {
                aux = 0;
                aux2 = -1;
            }

            angRotRueda += v / radioRueda;
            if (v == 5)
                angRotRueda = -5 / radioRueda;

            // rotacion rueda

            RuedaMeshIzq.Transform = Matrix.Scaling(Escala)
                    * Matrix.Translation(0, -cochePos.Y - 15, 0)
                    * Matrix.RotationX(angRotRueda * aux2)
                    * Matrix.Translation(0, cochePos.Y + 15, 0)
                    * Matrix.Translation(-OffsetRuedaIzq)

                    * MR

                    * Matrix.RotationY(aux)
                    * Matrix.Translation(cochePos);

            RuedaMeshDer.Transform = Matrix.Scaling(Escala)
                    * Matrix.Translation(0, -cochePos.Y - 15, 0)
                    * Matrix.RotationX(angRotRueda * aux2)
                    * Matrix.Translation(0, cochePos.Y + 15, 0)
                    * Matrix.Translation(OffsetRuedaDer)

                    * MR

                    * Matrix.RotationY(aux)
                    * Matrix.Translation(cochePos);
            /*
            var M1 = Matrix.Scaling(Escala)
                    * Matrix.Translation(0, -cochePos.Y - 15, 0)
                    * Matrix.RotationX(angRotRueda)
                    * Matrix.Translation(0, cochePos.Y + 15, 0)
                    * Matrix.Translation(-OffsetRuedaIzq)
                    * MR
                    * Matrix.RotationY()
                    Matrix.Translation(cochePos);

            

            RuedaMeshIzq.Transform = Matrix.Scaling(Escala)
                    * Matrix.Translation(0, -cochePos.Y - 15, 0)
                    * Matrix.RotationX(aux)
                    * Matrix.Translation(0, cochePos.Y + 15, 0)
                    * Matrix.Translation(-OffsetRuedaIzq)

                    * MR

                    * Matrix.RotationY((float)Math.PI)
                    * Matrix.Translation(cochePos);

    */
        }

        public void RotarRuedas(float velocidad, Vector3 escala)
        {
            //this.RuedaMeshDer.rotateX
            //var scale = Matrix.Scale(escala);
            //var rotacion = Matrix.RotationY()
            //TODO


            throw new NotImplementedException();
        }

        /// <summary>
        /// Solo lo hago si son las de adelante.
        /// </summary>
        public void GirarRuedas(float VolanteDireccion)
        {
            if (!SonDelanteras)
                return;
            //TODO
            throw new NotImplementedException();

        }
        public void Render()
        {
            RuedaMeshDer.render();
            RuedaMeshIzq.render();
            if (SonDelanteras)
            {
                TgcText2D Drawtext = new TgcText2D();
                Drawtext.drawText("I = " + TgcParserUtils.printVector3(RuedaMeshIzq.Position), 0, 100, Color.OrangeRed);
                Drawtext.drawText("D =" + TgcParserUtils.printVector3(RuedaMeshDer.Position), 0, 120, Color.OrangeRed);
            }
        }
    }
}
