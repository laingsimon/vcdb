namespace vcdb.IntegrationTests
{
    public class IntegrationTestScenario
    {
        public IntegrationTestScenario(IDatabaseProduct databaseProduct, IntegrationTestFileGroup fileGroup)
        {
            DatabaseProduct = databaseProduct;
            FileGroup = fileGroup;
        }

        internal IDatabaseProduct DatabaseProduct { get; }
        internal IntegrationTestFileGroup FileGroup { get; }

        public string Name
        {
            get
            {
                var directoryName = FileGroup.DirectoryPath.Name;
                var directoryNameShortened = directoryName.Length > FileGroup.Mode.ToString().Length + 1
                    ? directoryName.Substring(FileGroup.Mode.ToString().Length + 1)
                    : directoryName;

                return FileGroup.DatabaseVersion != null && FileGroup.DatabaseVersion.ToString() != DatabaseProduct.Name
                    ? $"{directoryNameShortened}^{FileGroup.DatabaseVersion.MinimumCompatibilityVersion}"
                    : directoryNameShortened;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
