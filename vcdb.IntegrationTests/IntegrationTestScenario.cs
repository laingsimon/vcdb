namespace vcdb.IntegrationTests
{
    public class IntegrationTestScenario
    {
        public string Name { get; set; }
        public string Mode { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
