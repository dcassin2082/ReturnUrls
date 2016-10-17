USE [autohausazdb]
GO

/****** Object:  Table [dbo].[member_pwd]    Script Date: 9/30/2016 4:15:24 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[member_pwd](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MemberNum] [int] NOT NULL,
	[PasswordHash] [binary](64) NOT NULL,
	[Salt] [uniqueidentifier] NULL,
 CONSTRAINT [PK_member_pwd] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[member_pwd]  WITH CHECK ADD  CONSTRAINT [FK_member_pwd_members] FOREIGN KEY([MemberNum])
REFERENCES [dbo].[members] ([MemberNum])
GO

ALTER TABLE [dbo].[member_pwd] CHECK CONSTRAINT [FK_member_pwd_members]
GO


