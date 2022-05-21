---
layout: post
title:  "Versioned stored procedues with Entity Framework"
date:   2022-05-10 10:27:52 +0200
categories: dotnet ef
---

## Entity Framework (EF)

[EF](https://docs.microsoft.com/en-us/ef/) is an awesome .Net library developed by Microsoft that allows your applications to interact with databases.

One of the best feature of the library is the possibility to [versionised](https://docs.microsoft.com/en-us/ef/core/managing-schemas/) your database. Its functioning is simple, you create classes that will represent your database tables, then generate a [migration](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations). Once you run this migration, your database schema will be updated accordingly.

In addition to this migration feature, EF provides methods to create, update and delete entities easily. Unfortunatly, nothing's perfect, an EF can suffer performance problems if you want do to complex tasks like doing bulk modifications or requests many condiditions.

To resolve these performance problems, you can use additional libraries like [this one](https://entityframework-extensions.net/bulk-insert), but for more specific cases, you will need a native SQL solution, like stored procedures.

I won't address views or functions in this article, but the principle will be exaclty the same.

## Stored procedures in EF

-- todo

## Code to versioned stored procedues

-- todo