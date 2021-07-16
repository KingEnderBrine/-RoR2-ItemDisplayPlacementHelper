namespace ItemDisplayPlacementHelper
{
    public enum EditMode { Move, Rotate, Scale, Combined }
    public enum EditSpace { Global, Local }
    public enum Axis
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 4,
        CameraPerpendicular = 8,
        CameraParallel = 16,
        XY = X | Y,
        XZ = X | Z,
        YZ = Y | Z,
        XYZ = X | Y | Z
    }
    public enum CopyFormat { Custom, Block, Inline, ForParsing }
}
