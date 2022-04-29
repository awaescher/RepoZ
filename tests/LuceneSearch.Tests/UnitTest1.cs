namespace LuceneSearch.Tests
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));

            var results = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

            hits.Should().Be(0);
        }

        [Fact]
        public async Task Tesdt1()
        {
            // arrange
            var sut = new RepositoryIndex(new LuceneDirectoryInstance(new RamLuceneDirectoryFactory()));
            var item = new RepositorySearchModel()
                {
                    Path = "c:/a/b/c",
                    RepositoryName = "RepoZ",
                    Tags = new List<string>()
                        {
                            "repositories",
                            "work",
                        },
                };
            await sut.ReIndexMediaFileAsync(item).ConfigureAwait(false);

            // act
            var result = sut.Search("tag:work project x", SearchOperator.Or, out var hits);

            // assert
            hits.Should().Be(1);
        }
    }
}