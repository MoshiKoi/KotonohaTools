# Kotonoha Data Generation

Database generation tool for [Kotonoha](https://github.com/MoshiKoi/Kotonoha)

As this tool is completely dedicated for use in the aforementioned project, there are no guarentees that the database format will be stable.

# Usage

```
unidic <file> <licence file> <database file>
```

# Note on the Wiktionary data

Wiktionary pages, while generally consistant, do vary somewhat, making parsing and data extraction difficult.
The Wiktionary parser is fairly conservative in what it matches, in the interests of not having random wiki markup included.
This means that it ignores a lot of entries when, for instance, it is unable to expand a template (template expansion is currently manually handled by a manually maintained dictionary rather than an actual parser)

# Notes on copyright and licencing

The two data sources currently used (JMdict and Wiktionary) are both licenced under a *Creative Commons Attribution-ShareAlike Licence*.
This means that this data must be properly attributed when redistributing.
To this end, every entry (technically, `Subentry`) in the database has a `Citation` record associated with it, holding the name and short description of the licence (as it is an attribution licence, the full licence text is not necessary, though a link is provided)
When using the database, please ensure that you use this record accordingly to display notices somewhere.

## JMdict

The JMdict additionally requires at least monthly updates (See: https://www.edrdg.org/edrdg/licence.html). If you use this tool, please be aware of this requirement.

> If a software package, WWW server, smartphone app, etc. uses the files or incorporates data from the files, there must be a procedure for regular updating of the data from the most recent versions available. For example, WWW-based dictionary servers should update their dictionary versions at least once a month. Failure to keep the versions up-to-date is a violation of the licence to use the data.

You could probably set a cronjob or something to automatically download the latest version and run the tool.

## Unidic

# Database format

For details on how this data is generated, see docs/generation.md.

