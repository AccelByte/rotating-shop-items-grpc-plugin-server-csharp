// Copyright (c) 2023 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;

using CommandLine;

using AccelByte.PluginArch.ItemRotation.Demo.Client.Model;

namespace AccelByte.PluginArch.ItemRotation.Demo.Client
{
    internal class Program
    {
        static int Main(string[] args)
        {
            int exitCode = 0;
            Parser.Default.ParseArguments<ApplicationConfig>(args)
                .WithParsed((config) =>
                {
                    config.FinalizeConfigurations();
                    PlatformWrapper wrapper = new PlatformWrapper(config);

                    Console.WriteLine($"\tBaseUrl: {config.BaseUrl}");
                    Console.WriteLine($"\tClientId: {config.ClientId}");
                    Console.WriteLine($"\tUsername: {config.Username}");
                    Console.WriteLine($"\tStore Category: {config.CategoryPath}");
                    if (config.GrpcServerUrl != "")
                        Console.WriteLine($"\tGrpc Target: {config.GrpcServerUrl}");
                    else if (config.ExtendAppName != "")
                        Console.WriteLine($"\tExtend App: {config.ExtendAppName}");
                    else
                    {
                        Console.WriteLine($"\tNO GRPC TARGET SERVER");
                        exitCode = 2;
                        return;
                    }

                    try
                    {
                        Console.Write("Logging in to AccelByte... ");
                        var userInfo = wrapper.Login();
                        Console.WriteLine("[OK]");
                        Console.WriteLine($"User: {userInfo.UserName}");

                        Console.Write("Configuring custom configuration... ");
                        wrapper.ConfigureGrpcTargetUrl();
                        Console.WriteLine("[OK]");
                        try
                        {
                            Console.Write("Check Currency... ");
                            wrapper.CheckAndCreateCurrencyIfNotExists();
                            Console.WriteLine("[OK]");

                            Console.Write("Creating draft store... ");
                            wrapper.CreateStore();
                            Console.WriteLine("[OK]");

                            Console.Write("Create store category... ");
                            wrapper.CreateCategory(config.CategoryPath);
                            Console.WriteLine("[OK]");

                            Console.Write("Create store view... ");
                            wrapper.CreateStoreView();
                            Console.WriteLine("[OK]");

                            Console.Write("Create store section with items... ");
                            SimpleSectionInfo section = wrapper.CreateSectionWithItems(10, config.CategoryPath);
                            Console.WriteLine("[OK]");
                            section.WriteToConsole();                            

                            Console.Write("Publishing store changes... ");
                            wrapper.PublishStoreChange();
                            Console.WriteLine("[OK]");

                            try
                            {
                                if (config.RunMode.Trim().ToLower() == "backfill")
                                {
                                    Console.Write("Enabling custom backfill for section... ");
                                    wrapper.EnableFixedRotationWithCustomBackfillForSection(section.Id);
                                    Console.WriteLine("[OK]");
                                }
                                else
                                {
                                    Console.Write("Enabling custom rotation for section... ");
                                    wrapper.EnableCustomRotationForSection(section.Id);
                                    Console.WriteLine("[OK]");
                                }

                                Console.Write("Publishing store changes... ");
                                wrapper.PublishStoreChange();
                                Console.WriteLine("[OK]");

                                Console.Write("Retrieving active sections's rotation items... ");
                                var aSections = wrapper.GetSectionRotatedItems(userInfo.UserId!);
                                Console.WriteLine("[OK]");
                                
                                foreach (var aSection in aSections)
                                    aSection.WriteToConsole();
                            }
                            catch (Exception x)
                            {
                                Console.WriteLine($"Exception: {x.Message}");
                                exitCode = 1;
                            }
                            finally
                            {
                                Console.Write("Removing section... ");
                                wrapper.DeleteSection(section);
                                Console.WriteLine("[OK]");
                            }
                        }
                        catch (Exception x)
                        {
                            Console.WriteLine($"Exception: {x.Message}");
                            exitCode = 1;
                        }
                        finally
                        {
                            Console.Write("Removing store view... ");
                            wrapper.DeleteStoreView();
                            Console.WriteLine("[OK]");

                            Console.Write("Deleting custom configuration... ");
                            wrapper.DeleteGrpcTargetUrl();
                            Console.WriteLine("[OK]");

                            Console.Write("Deleting store... ");
                            wrapper.DeleteStore();
                            Console.WriteLine("[OK]");
                        }
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"Exception: {x.Message}");
                        exitCode = 1;
                    }
                    finally
                    {
                        wrapper.Logout();
                    }
                })
                .WithNotParsed((errors) =>
                {
                    Console.WriteLine("Invalid argument(s)");
                    foreach (var error in errors)
                        Console.WriteLine($"\t{error}");
                    exitCode = 2;
                });

            return exitCode;
        }
    }
}