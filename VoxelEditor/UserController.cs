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
    class UserController : Component {

        public override void Update() {
            if (Keyboard.IsKeyDown(OpenTK.Input.Key.Space)) {
                var g = Game.ActiveScene.Init(new Mesh(Primitive.CubePosAndNormals));
                g.Transform.Position = Transform.Position;
            }

            if (Keyboard.IsKeyDown(OpenTK.Input.Key.W)) {
                Transform.Translate(0, 0, 1);
            }

        }

    }
}
