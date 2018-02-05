// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using IdentityServer4;
using Kentor.AuthServices;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.Metadata;
using Kentor.AuthServices.WebSso;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace QuickstartIdentityServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Options.GlobalEnableSha256XmlSignatures();
            services.AddMvc();

            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddTestUsers(Config.GetUsers());

            services.AddAuthentication(options =>
                options.DefaultScheme = IdentityServerConstants.DefaultCookieAuthenticationScheme
            )
            
                .AddCookie("Cookies")
                .AddYammer("Yammer", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClientId = "AjIMVaeup4jhDLzjFqFZdw";
                    options.ClientSecret = "rlp5TVCDeRGSzYhwZOjHCd6o7nO9QvsJiW0Pjq0Rw";
                })
                .AddOpenIdConnect("oidc", "OpenID Connect", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;
                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "implicit";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role",
                    };
                }).AddSaml2(options =>
                {
                    // Metadata negotiation
                    // if provider dosen't support dynamic metadata, we have to sign requests using thumbprint.
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SPOptions.EntityId = new Saml2NameIdentifier("http://localhost:5000/Saml2");
                    var identityProvider = new IdentityProvider(new EntityId("http://www.okta.com/exkmt9t3ikKnfoCQt2p6"),
                        options.SPOptions)
                    {
                        SingleSignOnServiceUrl = new Uri("https://readify.okta.com/app/name_testapp_1/exkmt9t3ikKnfoCQt2p6/sso/saml"),
                        Binding = Saml2BindingType.HttpRedirect,
                    };
                    identityProvider.SigningKeys.AddConfiguredKey(new X509Certificate2("okta.cert"));
                    options.IdentityProviders.Add(identityProvider);
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}