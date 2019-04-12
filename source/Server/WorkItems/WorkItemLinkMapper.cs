﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Octopus.Server.Extensibility.Extensions.WorkItems;
using Octopus.Server.Extensibility.HostServices.Model.PackageMetadata;
using Octopus.Server.Extensibility.IssueTracker.Jira.Configuration;
using Octopus.Server.Extensibility.IssueTracker.Jira.Integration;
using Octopus.Server.Extensibility.Resources.IssueTrackers;

namespace Octopus.Server.Extensibility.IssueTracker.Jira.WorkItems
{
    public class WorkItemLinkMapper : IWorkItemLinkMapper
    {
        private readonly IJiraConfigurationStore store;
        private readonly CommentParser commentParser;
        private readonly IJiraRestClient jira;

        public WorkItemLinkMapper(IJiraConfigurationStore store,
            CommentParser commentParser,
            IJiraRestClient jira)
        {
            this.store = store;
            this.commentParser = commentParser;
            this.jira = jira;
        }

        public string CommentParser => JiraConfigurationStore.CommentParser;
        public bool IsEnabled => store.GetIsEnabled();

        public WorkItemLink[] Map(OctopusPackageMetadata packageMetadata)
        {
            if (packageMetadata.CommentParser != CommentParser)
                return null;

            var baseUrl = store.GetBaseUrl();
            if (string.IsNullOrWhiteSpace(baseUrl))
                return null;

            var isEnabled = store.GetIsEnabled();

            var releaseNotePrefix = store.GetReleaseNotePrefix();
            var workItemIds = commentParser.ParseWorkItemIds(packageMetadata).Distinct();

            return workItemIds.Select(workItemId => new WorkItemLink
                {
                    Id = workItemId,
                    Description = isEnabled ? GetReleaseNote(workItemId, releaseNotePrefix) : workItemId,
                    LinkUrl = isEnabled ? baseUrl + "/browse/" + workItemId : null
                })
                .ToArray();
        }

        string GetReleaseNote(string workItemId, string releaseNotePrefix)
        {
            var issue = jira.GetIssue(workItemId).Result;
            if (issue is null) return workItemId;
            
            if (issue.Fields.Comments.Total == 0 || string.IsNullOrWhiteSpace(releaseNotePrefix))
                return issue.Fields.Summary;

            var releaseNoteRegex = new Regex($"^{releaseNotePrefix}", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var issueComments = jira.GetIssueComments(workItemId).Result;

            var releaseNote = issueComments?.Comments.LastOrDefault(c => releaseNoteRegex.IsMatch(c.Body))?.Body;
            return !string.IsNullOrWhiteSpace(releaseNote)
                ? releaseNoteRegex.Replace(releaseNote, "")
                : issue.Fields.Summary ?? workItemId;
        }
    }
}