using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Maliev.CareerService.Infrastructure.ValueGenerators;

public class RowVersionGenerator : ValueGenerator<byte[]>
{
    public override byte[] Next(EntityEntry entry) => Generate();

    public override bool GeneratesTemporaryValues => false;

    private static byte[] Generate()
    {
        var bytes = new byte[8];
        Random.Shared.NextBytes(bytes);
        return bytes;
    }
}
