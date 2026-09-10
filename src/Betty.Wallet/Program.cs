using Betty.Wallet;
using Betty.Wallet.Domain;
using Betty.Wallet.Game;

var app = new ConsoleApp(
    Console.In,
    Console.Out,
    new Wallet(),
    new SlotGame(new SystemRandomNumberGenerator()));

return app.Run();
