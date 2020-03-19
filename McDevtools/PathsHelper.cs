using System;
using System.Collections.Generic;
using System.Text;

namespace McDevtools {
    internal static class PathsHelper {

        public static string mcDirectory => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + $"/.minecraft/";

        public static string mcSavesDirectory => mcDirectory + "saves/";

    }
}
