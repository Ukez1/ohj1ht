#region Using Statements

using System;
#endregion

namespace CrystalDungeon.HT
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using var game = new CrystalDungeonPeli();
            game.Run();
        }
    }
}
