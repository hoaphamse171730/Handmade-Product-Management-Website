using AutoMapper;
using Firebase.Auth;
using FluentValidation;
using Google.Apis.Storage.v1.Data;
using HandmadeProductManagement.Contract.Repositories.Entity;
using HandmadeProductManagement.Contract.Repositories.Interface;
using HandmadeProductManagement.Contract.Services.Interface;
using HandmadeProductManagement.Core.Base;
using HandmadeProductManagement.Core.Common;
using HandmadeProductManagement.Core.Constants;
using HandmadeProductManagement.ModelViews.UserInfoModelViews;
using HandmadeProductManagement.Repositories.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace HandmadeProductManagement.Services.Service
{
    public class UserInfoService : IUserInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IValidator<UserInfoForUpdateDto> _updateValidator;

        public UserInfoService(IUnitOfWork unitOfWork, IMapper mapper, IValidator<UserInfoForUpdateDto> updateValidator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _updateValidator = updateValidator;
        }

        public async Task<bool> UploadUserAvatar(IFormFile file, string userId)
        {
            if (file == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageFileNotFound);
            }

            var user = await _unitOfWork.GetRepository<ApplicationUser>()
                .Entities
                .Include(u => u.UserInfo)
                .FirstOrDefaultAsync(u => u.Id.ToString() == userId)
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            if (file.Length == 0)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageFileEmpty);
            }

            var uploadImageService = new ManageFirebaseImageService();

            using (var stream = file.OpenReadStream())
            {
                var fileName = $"{Guid.NewGuid()}_{file.FileName}";
                var imageUrl = await uploadImageService.UploadFileAsync(stream, fileName);

                if (user.UserInfo == null)
                {
                    throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserInfoNotFound);
                }

                user.UserInfo.AvatarUrl = imageUrl;

                _unitOfWork.GetRepository<UserInfo>().Update(user.UserInfo);
            }

            await _unitOfWork.SaveAsync();

            return true;
        }

        public async Task<UserInfoDto> GetUserInfoByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Include(au => au.UserInfo)
                .FirstOrDefaultAsync(au => au.Id == Guid.Parse(id))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            if (applicationUser.UserInfo == null)
            {
                throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserInfoNotFound);
            }

            var userInfoDto = _mapper.Map<UserInfoDto>(applicationUser.UserInfo);

            userInfoDto.PhoneNumber = applicationUser.PhoneNumber ?? string.Empty;

            return userInfoDto;
        }

        public async Task<bool> PatchUserInfoAsync(string id, UserInfoForUpdateDto request)
        {
            var patchDto = request!;

            // Validate
            var validationResult = await _updateValidator.ValidateAsync(patchDto);

            if (!validationResult.IsValid)
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), validationResult.Errors.Select(e => e.ErrorMessage).FirstOrDefault() ?? string.Empty);
            }

            if (!string.IsNullOrWhiteSpace(patchDto.Address))
            {
                // Regex allowing Vietnamese letters, numbers, spaces, commas, and periods
                if (Regex.IsMatch(patchDto.Address, @"[^A-Za-z0-9À-ỹ\s,\.]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidAddressFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(patchDto.FullName))
            {
                // Regex allowing only Vietnamese letters and spaces
                if (Regex.IsMatch(patchDto.FullName, @"[^A-Za-zÀ-ỹ\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
                }
            }

            if (!string.IsNullOrWhiteSpace(patchDto.Bio))
            {
                // Regex allowing only Vietnamese letters and spaces
                if (Regex.IsMatch(patchDto.Bio, @"[^A-Za-zÀ-ỹ\s]"))
                {
                    throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidCustomerNameFormat);
                }
            }

            if (!Guid.TryParse(id, out _))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageInvalidGuidFormat);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                throw new BaseException.BadRequestException(StatusCodeHelper.BadRequest.ToString(), Constants.ErrorMessageEmptyId);
            }

            var applicationUser = await _unitOfWork.GetRepository<ApplicationUser>().Entities
                .Include(au => au.UserInfo)
                .FirstOrDefaultAsync(au => au.Id == Guid.Parse(id))
                ?? throw new BaseException.NotFoundException(StatusCodeHelper.NotFound.ToString(), Constants.ErrorMessageUserNotFound);

            var userInfo = applicationUser.UserInfo;

            // Use a dictionary to iterate over the patchDto properties
            var patchData = new Dictionary<string, object?>
            {
                { "FullName", patchDto.FullName },
                { "DisplayName", patchDto.DisplayName },
                { "Bio", patchDto.Bio },
                { "BankAccount", patchDto.BankAccount },
                { "BankAccountName", patchDto.BankAccountName },
                { "Bank", patchDto.Bank },
                { "Address", patchDto.Address },
            };

            // Iterate over the dictionary and update fields if they are not null
            foreach (var field in patchData)
            {
                if (field.Value != null)
                {
                    var propertyInfo = typeof(UserInfo).GetProperty(field.Key);
                    propertyInfo?.SetValue(userInfo, field.Value);
                }
            }

            userInfo.LastUpdatedTime = DateTime.UtcNow;
            userInfo.LastUpdatedBy = id;

            _unitOfWork.GetRepository<UserInfo>().Update(userInfo);
            await _unitOfWork.SaveAsync();

            return true;
        }

    }
}
