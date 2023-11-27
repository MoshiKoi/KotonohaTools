# Overview

Kotonoha collects the following information from each source:

## JMdict

### Entry

 - Sense[]
 - Kanji[]
 - Reading[]

### Sense
 - Restricted to (reading, kanji)
 - Part of Speech
 - Gloss[]

Because this forms the base structure of the database, all of the fields map directly to the corresponding field in the database.

## Wiktionary

### Entry
 - Etymology[]

### Etymology
 - Kanji
 - Reading
 - Pitch Accent
 - Part of speech
 - Gloss[]

Wiktionary's "Etymology" sections roughly correspond to JMdict senses.
However, to match JMdict, the Kanji and Reading fields are placed on the containing Entry, with Kanji and Reading being merged into the "Restricted to (reading, kanji)" field instead

## Unidic
 - Kanji
 - Reading
 - Pitch Accent
