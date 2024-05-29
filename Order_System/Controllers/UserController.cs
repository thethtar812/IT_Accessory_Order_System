﻿using Microsoft.AspNetCore.Mvc;
using Order_System.Service;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using Order_System.Enums;
using Order_System.Models.User;

namespace Order_System.Controllers;

public class UserController : ControllerBase
{
    private readonly AdoDotNetService _adoDotNetService;

    public UserController(AdoDotNetService adoDotNetService)
    {
        _adoDotNetService = adoDotNetService;
    }

    [HttpPost]
    [Route("/api/account/register")]

    public IActionResult Register([FromBody] RegisterRequestModel requestModel)
    {
        try
        {
            #region Validation

            if (string.IsNullOrEmpty(requestModel.FirstName))
                return BadRequest("FirstName cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.LastName))
                return BadRequest(" LastName cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.PhoneNo))
                return BadRequest("PhoneNumber cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Password))
                return BadRequest("Password cannot be empty.");

            #endregion

            #region Email Duplicate Testing

            string duplicateQuery = @"SELECT [UserId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[PhoneNo]
      ,[Password]
      ,[UserRole]
      ,[IsActive]
  FROM [dbo].[Users] WHERE Email = @Email AND IsActive = @IsActive";
            List<SqlParameter> duplicateParams = new()
            {
                new SqlParameter("@Email", requestModel.Email),
                new SqlParameter("@IsActive", true)
            };
            DataTable dt = _adoDotNetService.QueryFirstOrDefault(duplicateQuery, duplicateParams.ToArray());

            if (dt.Rows.Count > 0)
                return Conflict("User with this email already exists!");

            #endregion

            #region Register Case

            string query = @"INSERT INTO [dbo].[Users]
           ([FirstName]
           ,[LastName]
           ,[Email]
           ,[PhoneNo]
           ,[Password]
           ,[UserRole]
           ,[IsActive])
VALUES (@FirstName, @LastName, @Email, @PhoneNo, @Password, @UserRole, @IsActive)";
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@FirstName", requestModel.FirstName),
                new SqlParameter("@LastName", requestModel.LastName),
                new SqlParameter("@Email", requestModel.Email),
                new SqlParameter("@PhoneNo", requestModel.PhoneNo),
                new SqlParameter("@Password", requestModel.Password),
                new SqlParameter("@UserRole", EnumUserRoles.User.ToString()),
                new SqlParameter("@IsActive", true)
            };
            int result = _adoDotNetService.Execute(query, parameters.ToArray());

            #endregion

            return result > 0 ? StatusCode(201, "Registration Successful!") : BadRequest("Fail!");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Route("/api/account/login")]
    public IActionResult Login([FromBody] LoginRequestModel requestModel)
    {
        try
        {
            #region Validation

            if (string.IsNullOrEmpty(requestModel.Email))
                return BadRequest("Email cannot be empty.");

            if (string.IsNullOrEmpty(requestModel.Password))
                return BadRequest("Password cannot be empty.");

            #endregion

            string query = @"SELECT [UserId]
      ,[FirstName]
      ,[LastName]
      ,[Email]
      ,[PhoneNo]
      ,[Password]
      ,[UserRole]
      ,[IsActive]
  FROM [dbo].[Users] WHERE Email = @Email AND IsActive = @IsActive AND Password = @Password && UserRole = @UserRole";
            List<SqlParameter> parameters = new()
            {
                new SqlParameter("@Email", requestModel.Email),
                new SqlParameter("@Password", requestModel.Password),
                new SqlParameter("@UserRole", EnumUserRoles.User.ToString()),
                new SqlParameter("@IsActive", true),
            };
            DataTable user = _adoDotNetService.QueryFirstOrDefault(query, parameters.ToArray());

            if (user.Rows.Count == 0)
                return NotFound("User Not found.");

            return Ok(JsonConvert.SerializeObject(user));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}