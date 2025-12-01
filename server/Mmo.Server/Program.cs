using Mmo.Shared;

Console.WriteLine($"Starting {SharedConstants.GameName} Server...");
Console.WriteLine($"Protocol Version: {SharedConstants.ProtocolVersion}");
Console.WriteLine($"Default Port: {SharedConstants.DefaultPort}");
Console.WriteLine("Server started successfully! Press Ctrl+C to stop.");

// TODO: Implement actual server logic in future issues
await Task.Delay(Timeout.Infinite);
