using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EpsilonEngine;
using EpsilonEngine.Stdlib;
using EpsilonEngine.Graphics;

using Nums.Vectors;

namespace VoxelEditor {
    class Program {
        static void Main(string[] args) {


            var g = Game.ActiveScene.Init(new Camera(), new FlightMovment());
            //g.Transform.LookAt(OpenTK.Vector3.Zero);
            Game.ActiveScene.Init(new UserController(), new Mesh(Primitive.CubePosAndNormals));


            Game.Renderer.ShaderProgram.SetVec3("material.ambient", new Vec3(1, .5f, .31f));
            Game.Renderer.ShaderProgram.SetVec3("material.diffuse", new Vec3(1, .5f, .31f));
            Game.Renderer.ShaderProgram.SetVec3("material.specular", new Vec3(.5f, .5f, .5f));
            Game.Renderer.ShaderProgram.SetFloat("material.shininess", 32f);

            Game.Renderer.ShaderProgram.SetVec3("light.ambient", new Vec3(0.2f));
            Game.Renderer.ShaderProgram.SetVec3("light.diffuse", new Vec3(.5f));
            Game.Renderer.ShaderProgram.SetVec3("light.specular", new Vec3(1));

            Game.Run();

        }
    }
}
