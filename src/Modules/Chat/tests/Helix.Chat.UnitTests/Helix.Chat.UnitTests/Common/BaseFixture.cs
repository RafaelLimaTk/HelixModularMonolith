using Bogus;

namespace Helix.Chat.UnitTests.Common;

public class BaseFixture
{
    public Faker Faker { get; set; }

    protected BaseFixture()
        => Faker = new Faker("pt_BR");

    public bool GetRandomBoolean()
        => new Random().NextDouble() < 0.5;
}

