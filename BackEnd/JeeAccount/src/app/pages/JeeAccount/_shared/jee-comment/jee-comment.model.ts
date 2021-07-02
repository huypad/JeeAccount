export class TopicCommentDTO {
  Id: string;
  Username: string;
  IsEdit: boolean;
  Text: string;
  Attachs: Attach;
  Reactions: Reaction;
  UserReaction: string;
  UserReactionColor: string;
  LengthReaction: number;
  Comments: CommentDTO[];
  DateCreated: Date;
  ViewLengthComment: number;
  TotalLengthComment: number;

}

export class CommentDTO {
  Id: string;
  Username: string;
  IsEdit: boolean;
  Text: string;
  LengthReaction: number;
  UserReaction: string;
  UserReactionColor: string;
  IsUserReply: boolean;
  LengthReply: number;
  Replies: CommentDTO[];
  Attachs: Attach;
  ViewLengthComment: number;
  TotalLengthComment: number;
  DateCreated: Date;
}

export class TopicCommnet {
  Id: string;
  Username: string;
  IsEdit: boolean;
  Comments: Comment[];
  DateCreated: Date;
  Text: string;
  Attachs: Attach;
  Reactions: Reaction;

  constructor() {
    this.Id = '';
    this.Username = '';
    this.IsEdit = false;
    this.Comments = [];
    this.Text = '';
    this.Attachs = new Attach();
    this.Reactions = new Reaction();
  }
}

export class PostCommentModel {
  TopicCommentID: string;
  CommentID: string;
  ReplyCommentID: string;
  Text: string;
  Attachs: Attach;
  constructor() {
    this.TopicCommentID = '';
    this.CommentID = '';
    this.ReplyCommentID = '';
    this.Text = '';
    this.Attachs = new Attach();
  }
}

export class ReactionCommentModel {
  TopicCommentID: string;
  CommentID: string;
  ReplyCommentID: string;
  UserReaction: string;
  constructor() {
    this.TopicCommentID = '';
    this.CommentID = '';
    this.ReplyCommentID = '';
    this.UserReaction = '';
  }
}

export class Attach {
  Images: string[];
  Files: string[];
  Videos: string[];
  constructor() {
    this.Images = [];
    this.Files = [];
    this.Videos = [];
  }
}

export class Reaction {
  LikeReactions: string[];
  LoveReactions: string[];
  CareReactions: string[];
  HahaReactions: string[];
  SadReactions: string[];
  AngryReactions: string[];
  constructor() {
    this.LikeReactions = [];
    this.LoveReactions = [];
    this.CareReactions = [];
    this.HahaReactions = [];
    this.SadReactions = [];
    this.AngryReactions = [];
  }
}

export class UserCommentInfo {
  Username: string;
  FullName: string;
  Jobtitle: string;
  AvartarImgURL: string;
  PhoneNumber: string;
  Email: string;
  Display: string;

}

export class QueryFilterComment {
  ViewLengthComment: number;
}