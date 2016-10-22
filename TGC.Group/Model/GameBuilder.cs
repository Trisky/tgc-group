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
            float scale = 0.1f;
            TgcMesh rueda = CrearRueda(scale);
            rueda.move(0, -25, 0);
            TgcScene hummerScene = Loader.loadSceneFromFile(MediaDir + "Hummer\\Hummer-TgcScene.xml");
            TgcMesh hummerMesh = hummerScene.Meshes[0];

            Ruedas ruedasAdelante = new Ruedas(rueda, new Vector3(60, 3, -40), new Vector3(60, 3, 40), true, scale);
            Ruedas ruedasAtras = new Ruedas(rueda, new Vector3(-60, 3, -40), new Vector3(-60, 3, 40), false, scale);

            return new Auto("hummer", 100f, 5f, 3f, 5f, 2f,
                            hummerMesh, Gm,
                            ruedasAdelante, ruedasAtras, rueda);
        }

        public TgcMesh CrearRueda(float escala)
        {
            //TgcMesh rueda = Loader.loadSceneFromFile(MediaDir + "ModelosX\\rueda.x").Meshes[0];
            TgcMesh rueda = Loader.loadSceneFromFile(MediaDir + "Olla\\Olla-TgcScene.xml").Meshes[0];
            rueda.Scale = new Vector3(escala, escala, escala);
            //rueda.AutoTransformEnable = false;
            return rueda;
        }
    }
}
