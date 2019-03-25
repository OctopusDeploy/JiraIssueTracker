﻿using System.ComponentModel;
using Octopus.Data.Resources;
using Octopus.Data.Resources.Attributes;
using Octopus.Server.Extensibility.Extensions.Infrastructure.Configuration;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.Configuration
{
    [Description("Configure the Jira Issue Tracker. [Learn more](https://g.octopushq.com/JiraIssueTracker).")]
    public class JiraConfigurationResource : ExtensionConfigurationResource
    {
        public const string JiraBaseUrlDescription = "Enter the base url of your Jira instance.";

        [DisplayName("Jira Base Url")]
        [Description(JiraBaseUrlDescription)]
        [Writeable]
        public string BaseUrl { get; set; }
        
        [DisplayName("Jira Connect App Password")]
        [Description("Set the password for authenticating with the Jira Connect App (generated by the Jira Connect application upon installation in to your Jira instance)")]
        [Writeable]
        public SensitiveValue Password { get; set; }

        [DisplayName("Octopus Installation Id")]
        [Description("Copy and paste this Id when configuring the Jira Connect application")]
        [ReadOnly(true)]
        public string OctopusInstallationId { get; set; }
    }
}