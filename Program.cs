using System;
using FirstEngine;
using WindowEngine;

namespace WindowEngine
{
  class Program
  {
    static void Main(string[] args)
    {
      using (Game game = new Game())
      {
        game.Run();
      }
    }
  }
}