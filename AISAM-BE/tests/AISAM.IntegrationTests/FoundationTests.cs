using AISAM.API.Controllers;
using AISAM.Common;
using AISAM.Common.Config;
using AISAM.Common.Dtos;
using AISAM.Common.Dtos.Request;
using AISAM.Common.Dtos.Response;
using AISAM.Common.Models;
using AISAM.Data.Enumeration;
using AISAM.Data.Model;
using AISAM.Repositories.IRepositories;
using AISAM.Services.IServices;
using AISAM.Services.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;

namespace AISAM.IntegrationTests;

public class FoundationTests
{
    [Fact]
    public async Task ProfileController_ReturnsForbidden_WhenRouteUserDoesNotMatchJwtUser()
    {
        var jwtUserId = Guid.NewGuid();
        var routeUserId = Guid.NewGuid();
        var controller = new ProfileController(new NoopProfileService(), NullLogger<ProfileController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = CreatePrincipal(jwtUserId)
            }
        };

        var result = await controller.GetUserProfiles(routeUserId);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal((int)HttpStatusCode.Forbidden, objectResult.StatusCode);
    }

    [Fact]
    public async Task ProfileService_ReturnsNotFound_WhenProfileIsOwnedByDifferentUser()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Name = "Owner profile",
            ProfileType = ProfileTypeEnum.Free
        };

        var profileRepository = new FakeProfileRepository(profile);
        var service = new ProfileService(profileRepository, new FakeUserRepository(ownerId, requesterId));

        var result = await service.GetProfileByIdAsync(profile.Id, requesterId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task ProfileService_DoesNotDelete_WhenProfileIsOwnedByDifferentUser()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Name = "Owner profile",
            ProfileType = ProfileTypeEnum.Free
        };

        var profileRepository = new FakeProfileRepository(profile);
        var service = new ProfileService(profileRepository, new FakeUserRepository(ownerId, requesterId));

        var result = await service.DeleteProfileAsync(profile.Id, requesterId);

        Assert.False(result.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(profileRepository.DeleteCalled);
    }

    [Fact]
    public async Task ProfileService_AllowsOwnedProfileLifecycle()
    {
        var ownerId = Guid.NewGuid();
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Name = "Original profile",
            ProfileType = ProfileTypeEnum.Free
        };

        var profileRepository = new FakeProfileRepository(profile);
        var service = new ProfileService(profileRepository, new FakeUserRepository(ownerId));

        var readResult = await service.GetProfileByIdAsync(profile.Id, ownerId);
        var updateResult = await service.UpdateProfileAsync(
            profile.Id,
            ownerId,
            new UpdateProfileRequest { Name = "Updated profile" });
        var deleteResult = await service.DeleteProfileAsync(profile.Id, ownerId);
        var restoreResult = await service.RestoreProfileAsync(profile.Id, ownerId);

        Assert.True(readResult.Success);
        Assert.True(updateResult.Success);
        Assert.True(profileRepository.UpdateCalled);
        Assert.Equal("Updated profile", updateResult.Data!.Name);
        Assert.True(deleteResult.Success);
        Assert.True(profileRepository.DeleteCalled);
        Assert.True(restoreResult.Success);
        Assert.True(profileRepository.RestoreCalled);
    }

    [Fact]
    public async Task ProfileService_DoesNotUpdateOrRestore_WhenProfileIsOwnedByDifferentUser()
    {
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = ownerId,
            Name = "Owner profile",
            ProfileType = ProfileTypeEnum.Free
        };

        var profileRepository = new FakeProfileRepository(profile);
        var service = new ProfileService(profileRepository, new FakeUserRepository(ownerId, requesterId));

        var updateResult = await service.UpdateProfileAsync(
            profile.Id,
            requesterId,
            new UpdateProfileRequest { Name = "Illegal update" });
        var restoreResult = await service.RestoreProfileAsync(profile.Id, requesterId);

        Assert.False(updateResult.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, updateResult.StatusCode);
        Assert.False(profileRepository.UpdateCalled);
        Assert.False(restoreResult.Success);
        Assert.Equal((int)HttpStatusCode.NotFound, restoreResult.StatusCode);
        Assert.False(profileRepository.RestoreCalled);
    }

    [Fact]
    public async Task ProfileService_ReturnsError_WhenAvatarFileIsProvided()
    {
        var userId = Guid.NewGuid();
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Existing profile",
            ProfileType = ProfileTypeEnum.Free
        };

        var profileRepository = new FakeProfileRepository(profile);
        var service = new ProfileService(profileRepository, new FakeUserRepository(userId));
        await using var stream = new MemoryStream(new byte[] { 1 });
        var createRequest = new CreateProfileRequest
        {
            Name = "Test profile",
            ProfileType = ProfileTypeEnum.Free,
            AvatarFile = new FormFile(stream, 0, stream.Length, "avatar", "avatar.png")
        };
        await using var updateStream = new MemoryStream(new byte[] { 2 });
        var updateRequest = new UpdateProfileRequest
        {
            AvatarFile = new FormFile(updateStream, 0, updateStream.Length, "avatar", "avatar.png")
        };

        var createResult = await service.CreateProfileAsync(userId, createRequest);
        var updateResult = await service.UpdateProfileAsync(profile.Id, userId, updateRequest);

        Assert.False(createResult.Success);
        Assert.Contains("upload is not enabled", createResult.Message);
        Assert.False(updateResult.Success);
        Assert.Contains("upload is not enabled", updateResult.Message);
        Assert.False(profileRepository.UpdateCalled);
    }

    [Fact]
    public async Task ProductService_ReturnsError_WhenImageFilesAreProvided()
    {
        var userId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var brand = CreateOwnedBrand(brandId, userId);
        brand.WorkspaceId = workspaceId;
        var product = new Product
        {
            Id = Guid.NewGuid(),
            BrandId = brandId,
            Brand = brand,
            Name = "Existing product"
        };

        var productRepository = new FakeProductRepository(product);
        var service = new ProductService(productRepository, new FakeBrandRepository(brand));
        await using var createStream = new MemoryStream(new byte[] { 1 });
        await using var updateStream = new MemoryStream(new byte[] { 2 });

        var createResult = await service.CreateAsync(workspaceId, userId, new ProductCreateRequest
        {
            BrandId = brandId,
            Name = "New product",
            ImageFiles = new List<IFormFile>
            {
                new FormFile(createStream, 0, createStream.Length, "image", "product.png")
            }
        });
        var updateResult = await service.UpdateAsync(product.Id, workspaceId, userId, new ProductUpdateRequestDto
        {
            ImageFiles = new List<IFormFile>
            {
                new FormFile(updateStream, 0, updateStream.Length, "image", "product.png")
            }
        });

        Assert.False(createResult.Success);
        Assert.Contains("upload is not enabled", createResult.Message);
        Assert.False(productRepository.AddCalled);
        Assert.False(updateResult.Success);
        Assert.Contains("upload is not enabled", updateResult.Message);
        Assert.False(productRepository.UpdateCalled);
    }

    [Fact]
    public async Task EmailService_ReturnsFalse_WhenSmtpIsNotConfigured()
    {
        var emailService = new EmailService(
            Options.Create(new EmailSettings()),
            Options.Create(new FrontendSettings { BaseUrl = "http://localhost:3000" }),
            NullLogger<EmailService>.Instance);

        var result = await emailService.SendEmailAsync(
            "user@example.com",
            "Subject",
            "<p>Body</p>",
            "Body");

        Assert.False(result);
    }

    private static ClaimsPrincipal CreatePrincipal(Guid userId)
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "Test"));
    }

    private static Brand CreateOwnedBrand(Guid brandId, Guid userId)
    {
        var profile = new Profile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Owner profile",
            ProfileType = ProfileTypeEnum.Free
        };

        return new Brand
        {
            Id = brandId,
            ProfileId = profile.Id,
            Profile = profile,
            Name = "Owned brand"
        };
    }

    private sealed class NoopProfileService : IProfileService
    {
        public Task<GenericResponse<ProfileResponseDto>> GetProfileByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<ProfileResponseDto>.CreateError("Not implemented"));
        }

        public Task<GenericResponse<IEnumerable<ProfileResponseDto>>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<IEnumerable<ProfileResponseDto>>.CreateSuccess(Array.Empty<ProfileResponseDto>()));
        }

        public Task<GenericResponse<ProfileResponseDto>> CreateProfileAsync(Guid userId, CreateProfileRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<ProfileResponseDto>.CreateError("Not implemented"));
        }

        public Task<GenericResponse<ProfileResponseDto>> UpdateProfileAsync(Guid id, Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<ProfileResponseDto>.CreateError("Not implemented"));
        }

        public Task<GenericResponse<bool>> DeleteProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<bool>.CreateError("Not implemented"));
        }

        public Task<GenericResponse<bool>> RestoreProfileAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GenericResponse<bool>.CreateError("Not implemented"));
        }
    }

    private sealed class FakeProfileRepository : IProfileRepository
    {
        private readonly Dictionary<Guid, Profile> _profiles;

        public FakeProfileRepository(params Profile[] profiles)
        {
            _profiles = profiles.ToDictionary(profile => profile.Id);
        }

        public bool DeleteCalled { get; private set; }
        public bool RestoreCalled { get; private set; }
        public bool UpdateCalled { get; private set; }

        public Task<Profile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _profiles.TryGetValue(id, out var profile);
            return Task.FromResult(profile);
        }

        public Task<Profile?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _profiles.TryGetValue(id, out var profile);
            return Task.FromResult(profile);
        }

        public Task<Profile?> GetFirstByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.Values
                .Where(profile => profile.UserId == userId && profile.Status != ProfileStatusEnum.Cancelled)
                .OrderBy(profile => profile.CreatedAt)
                .FirstOrDefault());
        }

        public Task<IEnumerable<Profile>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.Values.Where(profile => profile.UserId == userId).AsEnumerable());
        }

        public Task<IEnumerable<Profile>> GetByUserIdIncludingDeletedAsync(Guid userId, bool isDeleted, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.Values.Where(profile => profile.UserId == userId && profile.IsDeleted == isDeleted).AsEnumerable());
        }

        public Task<IEnumerable<Profile>> SearchUserProfilesAsync(Guid userId, string? searchTerm = null, bool? isDeleted = null, CancellationToken cancellationToken = default)
        {
            var query = _profiles.Values.Where(profile => profile.UserId == userId);
            if (isDeleted.HasValue)
            {
                query = query.Where(profile => profile.IsDeleted == isDeleted.Value);
            }

            return Task.FromResult(query.AsEnumerable());
        }

        public Task<Profile> CreateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<Profile> UpdateAsync(Profile profile, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            _profiles[profile.Id] = profile;
            return Task.FromResult(profile);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            DeleteCalled = true;
            if (!_profiles.TryGetValue(id, out var profile))
            {
                return Task.FromResult(false);
            }

            profile.Status = ProfileStatusEnum.Cancelled;
            return Task.FromResult(true);
        }

        public Task RestoreAsync(Guid id, CancellationToken cancellationToken = default)
        {
            RestoreCalled = true;
            if (_profiles.TryGetValue(id, out var profile))
            {
                profile.Status = ProfileStatusEnum.Pending;
            }

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_profiles.ContainsKey(id));
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users;

        public FakeUserRepository(params Guid[] userIds)
        {
            _users = userIds.ToDictionary(
                userId => userId,
                userId => new User
                {
                    Id = userId,
                    Email = $"{userId:N}@example.com",
                    PasswordHash = "hash",
                    PasswordSalt = "salt"
                });
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            _users.TryGetValue(id, out var user);
            return Task.FromResult(user);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return Task.FromResult(_users.Values.FirstOrDefault(user => user.Email == email));
        }

        public Task<User> CreateAsync(User user)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<User> UpdateAsync(User user)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }

        public Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return Task.FromResult(_users.Values.FirstOrDefault(user => user.PasswordResetToken == token));
        }

        public Task<User?> GetByEmailVerificationTokenAsync(string token)
        {
            return Task.FromResult(_users.Values.FirstOrDefault(user => user.EmailVerificationToken == token));
        }

        public Task<PagedResult<UserListDto>> GetPagedUsersAsync(PaginationRequest request)
        {
            var users = _users.Values.Select(user => new UserListDto
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                SocialAccountsCount = 0
            }).ToList();

            return Task.FromResult(new PagedResult<UserListDto>
            {
                Data = users,
                TotalCount = users.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }

    private sealed class FakeBrandRepository : IBrandRepository
    {
        private readonly Dictionary<Guid, Brand> _brands;

        public FakeBrandRepository(params Brand[] brands)
        {
            _brands = brands.ToDictionary(brand => brand.Id);
        }

        public Task<Brand?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _brands.TryGetValue(id, out var brand);
            return Task.FromResult(brand);
        }

        public Task<Brand?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _brands.TryGetValue(id, out var brand);
            return Task.FromResult(brand);
        }

        public Task<PagedResult<Brand>> GetPagedByProfileIdAsync(Guid profileId, PaginationRequest request, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var brands = _brands.Values.Where(brand => brand.ProfileId == profileId).ToList();
            return Task.FromResult(new PagedResult<Brand>
            {
                Data = brands,
                TotalCount = brands.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<Brand> AddAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            _brands[brand.Id] = brand;
            return Task.FromResult(brand);
        }

        public Task UpdateAsync(Brand brand, CancellationToken cancellationToken = default)
        {
            _brands[brand.Id] = brand;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly Dictionary<Guid, Product> _products;

        public FakeProductRepository(params Product[] products)
        {
            _products = products.ToDictionary(product => product.Id);
        }

        public bool AddCalled { get; private set; }
        public bool UpdateCalled { get; private set; }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        public Task<Product?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        public Task<PagedResult<Product>> GetPagedAsync(PaginationRequest request, Guid? brandId = null, bool includeDeleted = false, CancellationToken cancellationToken = default)
        {
            var products = _products.Values
                .Where(product => !brandId.HasValue || product.BrandId == brandId.Value)
                .ToList();

            return Task.FromResult(new PagedResult<Product>
            {
                Data = products,
                TotalCount = products.Count,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }

        public Task<IEnumerable<Product>> GetProductsByBrandIdAsync(Guid brandId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.Values.Where(product => product.BrandId == brandId).AsEnumerable());
        }

        public Task<IEnumerable<Product>> GetProductsByBrandIdIncludingDeletedAsync(Guid brandId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products.Values.Where(product => product.BrandId == brandId).AsEnumerable());
        }

        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            AddCalled = true;
            _products[product.Id] = product;
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
        {
            UpdateCalled = true;
            _products[product.Id] = product;
            return Task.CompletedTask;
        }
    }
}
