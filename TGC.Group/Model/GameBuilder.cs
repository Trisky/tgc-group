﻿using Microsoft.DirectX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TGC.Core.SceneLoader;
using TGC.Core.Text;
using TGC.Group.Model;

namespace TGC.GroupoMs.Model
{
    public class GameBuilder
    {
        public string MediaDir { get; set; }
        public GameModel Gm { get; set; }
        public TgcSceneLoader Loader { get; set; }


        public GameBuilder(string md, GameModel gm, TgcSceneLoader ld)
        {
            MediaDir = md;
            Gm = gm;
            Loader = ld;
        }


        public Auto CrearHummer(TgcScene MapScene)
        {
            float scaleRuedas = 1f;
            TgcMesh rueda = CrearRueda(1f);
            rueda.move(0, -25, 0);
            TgcScene hummerScene = Loader.loadSceneFromFile(MediaDir + "Hummer\\Hummer-TgcScene.xml");
            TgcMesh hummerMesh = hummerScene.Meshes[0];
            hummerMesh.Scale = new Vector3(0.4f,0.4f,0.4f);

            //para las ruedas el offset es (ancho,altura,largo)
            float y = 13f;
            Ruedas  ruedasAtras   = new Ruedas(rueda, new Vector3(40, y, 54), new Vector3(-40, y, 54), true, scaleRuedas);
            Ruedas ruedasAdelante = new Ruedas(rueda, new Vector3(-40, y, -62), new Vector3(40, y, -62), false, scaleRuedas);

            return new Auto("hummer", 100f, 5f, 3f, 5f, 2f,
                            hummerMesh, Gm,
                            ruedasAdelante, ruedasAtras, rueda);
        }

        public TgcMesh CrearRueda(float escala)
        {
            //TgcMesh rueda = Loader.loadSceneFromFile(MediaDir + "ModelosX\\rueda.x").Meshes[0];
            TgcMesh rueda = Loader.loadSceneFromFile(MediaDir + "Rueda\\Rueda-TgcScene.xml").Meshes[0];
            rueda.Scale = new Vector3(escala, escala, escala);
            //rueda.AutoTransformEnable = false;
            return rueda;
        }
    }
}