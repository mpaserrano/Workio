using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Services.Search;
using Moq;
using Workio.Controllers;

namespace Workio.Tests.Controller
{
    public class SearchControllerTests
    {
        private Mock<ISearchService> _searchService;
        private SearchController _controller;

        [Fact]
        public void SetUp()
        {
            _searchService = new Mock<ISearchService>();
            _controller = new SearchController(_searchService.Object);

        }


        [Fact]
        public void SearchController_Index_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act
            var result = _controller.Index("AAAAAAAAAAAAAAAAAA");
            var result1 = _controller.Index("AAAAAAAAA@AAAAAAAA");

            //Assert
            Assert.NotNull(result);
            Assert.NotNull(result1);
        }
    }
}
