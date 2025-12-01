using Godot;
using Mmo.Shared;

namespace GodotProject.Scripts;

/// <summary>
/// Main entry point for the 2DMMO Client.
/// </summary>
public partial class Main : Node2D
{
    public override void _Ready()
    {
        GD.Print($"Starting {SharedConstants.GameName} Client...");
        GD.Print($"Protocol Version: {SharedConstants.ProtocolVersion}");
        GD.Print($"Connecting to port: {SharedConstants.DefaultPort}");
        GD.Print("Client started successfully!");
    }

    public override void _Process(double delta)
    {
        // TODO: Implement game loop logic in future issues
    }
}
