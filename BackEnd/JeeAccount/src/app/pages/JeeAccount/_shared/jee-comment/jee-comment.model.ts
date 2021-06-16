export interface TopicCommnet {
  Id: string;
  Username: string;
  IsEdit: boolean;
  Comments: Comment[];
  DateCreated: any;
}

export class Comment {
  Id: string;
  Username: string;
  IsEdit: boolean;
  Text: string;
  Attachs: Attach;
  Reactions: Reaction;
  constructor() {
    this.Id = '';
    this.Username = '';
    this.IsEdit = false;
    this.Text = '';
    this.Attachs = new Attach();
    this.Reactions = new Reaction();
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