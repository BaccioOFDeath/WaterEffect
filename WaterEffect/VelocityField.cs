public class VelocityField
{
    public float[,] X { get; }
    public float[,] Y { get; }

    public int Size { get; }

    public VelocityField(int size)
    {
        Size = size;
        X = new float[size, size];
        Y = new float[size, size];
    }

    public void Clear()
    {
        System.Array.Clear(X, 0, X.Length);
        System.Array.Clear(Y, 0, Y.Length);
    }
}
