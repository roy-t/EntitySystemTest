using EntitySystemTest.Systems;

namespace EntitySystemTest.Mocks
{
    public class ComponentA : IComponent
    {
        public ComponentA(int entity, string name)
        {
            this.Entity = entity;
            this.Name = name;
        }
        public int Entity { get; set; }
        public string Name { get; }
    }

    public class ComponentB : IComponent
    {
        public ComponentB(int entity, string name)
        {
            this.Entity = entity;
            this.Name = name;
        }
        public int Entity { get; }
        public string Name { get; }
    }
}
