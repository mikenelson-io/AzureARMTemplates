USE [bjdpdfdb]
GO

/****** Object:  Table [dbo].[orders]    Script Date: 12/19/2016 2:00:51 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [dbo].[orders](
	[OrderId] [int] NOT NULL,
	[OrderDate] [datetime] NULL,
	[FirstName] [varchar](255) NULL,
	[LastName] [varchar](255) NULL,
	[Address] [varchar](255) NULL,
	[City] [varchar](255) NULL,
	[State] [varchar](255) NULL,
	[PostalCode] [varchar](255) NULL,
	[Country] [varchar](255) NULL,
	[Phone] [varchar](255) NULL,
	[Email] [varchar](255) NULL,
	[Total] [decimal](18, 0) NULL
)

GO
ALTER Table orders ADD PRIMARY KEY(OrderId)
SET ANSI_PADDING OFF
GO


