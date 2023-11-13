using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workio.Services.Interfaces;
using Moq;
using Workio.Controllers;

namespace Workio.Tests.Controller
{
    public class BlockedUsersControllerTests
    {
        private Mock<IBlockService> _blockService;
        private Mock<IUserService> _userService;

        private BlockedUsersController _controller;

        [Fact]
        public void SetUp()
        {
            _blockService = new Mock<IBlockService>();
            _userService = new Mock<IUserService>();
            _controller = new BlockedUsersController(_blockService.Object, _userService.Object);
        }

        [Fact]
        public void BlockedUsersController_BlockUser_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.BlockUser(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void BlockedUsersController_BlockedUsers_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.BlockedUsers();

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void BlockedUsersController_Unblock_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.Unblock(new Guid());

            //Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void BlockedUsersController_UnblockFromConnections_ReturnsSuccess()
        {
            //SetUp
            SetUp();

            //Act

            var result = _controller.UnblockFromConnections(new Guid());

            //Assert
            Assert.NotNull(result);
        }
    }
}
