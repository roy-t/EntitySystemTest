using EntitySystemTest.Systems;

namespace EntitySystemTest.Mocks
{
    public class SoloSystem : ISystemWithoutComponent
    {
        public static long Counter { get; set; }

        public void Process() => Counter++;
    }

    public class MonoSystem : ISystem<ComponentA>
    {
        public static long Counter { get; set; }

        public void Process(ComponentA component) => Counter++;
    }
    public class DuoSystem : ISystem<ComponentA, ComponentB>
    {
        public static long Counter { get; set; }

        public void Process(ComponentA component, ComponentB component2) => Counter++;
    }
}
