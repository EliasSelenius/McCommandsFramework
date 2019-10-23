using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EpsilonEngine;
using EpsilonEngine.Graphics;
using EpsilonEngine.Input;

using Nums.Vectors;

namespace VoxelEditor {
    class UserController : Component, IRenderable {
        
        

        public UserController() {
            this.EnableDraw(true);
        }

        public override void Update() {
        
        }

        public void Render() {
            //Game.Renderer.ShaderProgram.SetVec3("model", );
        }


    }


}
