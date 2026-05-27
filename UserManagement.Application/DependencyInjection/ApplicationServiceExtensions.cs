using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Users.Commands;
using UserManagement.Application.Users.Commands.Validators;
using UserManagement.Application.Common.Behaviors;

namespace UserManagement.Application.DependencyInjection;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));

        services.AddAutoMapper(cfg => cfg.AddProfile<UserMappingProfile>());

        services.AddValidatorsFromAssembly(typeof(CreateUserCommandValidator).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}