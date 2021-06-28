﻿using JeeAccount.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace JeeAccount.Services.CommentService
{
    public interface ICommentService
    {
        Task<string> GetTopicObjectID(string componentName);

        Task<HttpResponseMessage> CreateTopicObjectID(string username, string access_token);

        void SaveTopicID(string componenName, string topicID);

        Attach UpdateAllFileToMinio(Attach attachs, string username);

        Task PostCommentKafka(PostCommentModel postComment);
    }
}