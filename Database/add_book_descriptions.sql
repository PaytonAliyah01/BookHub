-- Add descriptions to all books in the BookHub database
-- This script adds meaningful descriptions based on known books and their authors

-- Update descriptions for specific well-known books
UPDATE Books 
SET Description = 'A powerful epistolary novel that explores themes of racism, domestic violence, and female empowerment in rural Georgia. Celie, the protagonist, writes letters to God documenting her struggles and eventual liberation from an abusive marriage.'
WHERE Title = 'The Color Purple' AND Author = 'Alice Walker';

UPDATE Books 
SET Description = 'A gripping Victorian crime novel following Sue Trinder, a young thief who becomes embroiled in an elaborate plot to defraud a wealthy heiress. Set in the grimy underworld of London and the Gothic mansions of the wealthy, this novel explores themes of deception, desire, and transformation.'
WHERE Title = 'Fingersmith' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'A vibrant coming-of-age novel about Nancy Astley, who falls in love with male impersonator Kitty Butler in the music halls of Victorian London. A bold exploration of sexuality, identity, and the underground lesbian culture of 1890s England.'
WHERE Title = 'Tipping the Velvet' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'A haunting tale set in a Victorian women''s prison where Margaret Prior, visiting as part of her charity work, becomes fascinated with Selina Dawes, a spiritualist medium. A dark exploration of love, madness, and the supernatural.'
WHERE Title = 'Affinity' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'Set in post-WWI London, this novel follows Frances and her mother as they take in lodgers to make ends meet. When two young men move in, their presence disrupts the carefully maintained household in unexpected and dangerous ways.'
WHERE Title = 'The Paying Guests' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'A chilling ghost story set in a crumbling English estate where Dr. Faraday becomes obsessed with the Ayres family and their mysterious ancestral home. A masterful blend of psychological thriller and supernatural horror.'
WHERE Title = 'The Little Stranger' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'Moving backwards through time from 1947 to 1941, this novel follows four Londoners whose lives intertwine during the dark days of World War II. A poignant exploration of love, loss, and survival during wartime.'
WHERE Title = 'The Night Watch' AND Author = 'Sarah Waters';

UPDATE Books 
SET Description = 'A semi-autobiographical novel about a young girl growing up in a strict evangelical household while struggling with her sexuality. A groundbreaking work that blends magical realism with sharp humor and social commentary.'
WHERE Title = 'Oranges Are Not the Only Fruit' AND Author = 'Jeanette Winterson';

UPDATE Books 
SET Description = 'An experimental novel that challenges conventional narrative structure while exploring a passionate love affair. The narrator''s beloved is dying of leukemia, and the book becomes a meditation on love, loss, and the nature of identity.'
WHERE Title = 'Written on the Body' AND Author = 'Jeanette Winterson';

UPDATE Books 
SET Description = 'Set in Napoleonic Europe, this magical realist novel follows Henri, a French soldier, and Villanelle, a Venetian boatman''s daughter with webbed feet. A surreal tale of passion, war, and the blurred lines between reality and fantasy.'
WHERE Title = 'The Passion' AND Author = 'Jeanette Winterson';

-- Update Natasha Lester books with historical fiction descriptions
UPDATE Books 
SET Description = 'Set during World War II, this novel follows Estella, a seamstress in Paris who becomes involved in the French Resistance. A tale of courage, love, and sacrifice as she fights against the Nazi occupation while protecting the secrets of her past.'
WHERE Title = 'The Paris Seamstress' AND Author = 'Natasha Lester';

UPDATE Books 
SET Description = 'A dual-timeline novel connecting a modern-day art curator with a World War II story of survival and love. When precious artworks surface decades later, they reveal a heart-wrenching tale of loss and the enduring power of hope.'
WHERE Title = 'If I Should Lose You' AND Author = 'Natasha Lester';

-- Update Rebecca Raisin books with romance/contemporary fiction descriptions
UPDATE Books 
SET Description = 'Follow Rosie as she travels the countryside in her vintage tea shop van, serving delicious treats and finding unexpected romance along the way. A heartwarming tale of following your dreams and discovering love in the most unexpected places.'
WHERE Title = 'Rosie''s Travelling Tea Shop' AND Author = 'Rebecca Raisin';

UPDATE Books 
SET Description = 'When Sarah inherits a charming bookshop filled with love stories, she discovers that the books might just help her write her own romantic happy ending. A cozy tale about the magic of books and the power of love.'
WHERE Title = 'The Little Bookshop of Love Stories' AND Author = 'Rebecca Raisin';

UPDATE Books 
SET Description = 'A delightful tale of a gingerbread café where sweet treats bring people together and create lasting friendships. When romance blooms among the sugar and spice, it proves that the best recipes include a dash of love.'
WHERE Title = 'The Gingerbread Café' AND Author = 'Rebecca Raisin';

UPDATE Books 
SET Description = 'Set in a charming bookshop on a picturesque corner, this novel follows Nina as she transforms a run-down shop into the heart of the community. A story about books, community, and finding home in unexpected places.'
WHERE Title = 'The Bookshop on the Corner' AND Author = 'Rebecca Raisin';

-- Update Katherine Reay books with contemporary/historical fiction descriptions
UPDATE Books 
SET Description = 'A contemporary novel about a bookshop owner who discovers family secrets through handwritten letters. As three women''s lives intersect through their shared love of literature, they find healing and hope in unexpected friendships.'
WHERE Title = 'The Printed Letter Bookshop' AND Author = 'Katherine Reay';

UPDATE Books 
SET Description = 'A charming contemporary romance following Jane Austen enthusiast Elizabeth as she escapes to an immersive Austen experience. But when reality intrudes on her fantasy, she must learn to write her own story.'
WHERE Title = 'The Austen Escape' AND Author = 'Katherine Reay';

UPDATE Books 
SET Description = 'A modern epistolary novel written as letters to Mr. Knightley from Jane Austen''s Emma. A young woman finds healing and love through literature as she navigates graduate school and discovers her own worth.'
WHERE Title = 'Dear Mr. Knightley' AND Author = 'Katherine Reay';

UPDATE Books 
SET Description = 'A dual-timeline novel set in London during WWII and present day. When Caroline inherits her grandmother''s house, she uncovers secrets about her family''s wartime experiences and finds unexpected connections to the past.'
WHERE Title = 'The London House' AND Author = 'Katherine Reay';

-- Update Alka Joshi books with historical fiction descriptions
UPDATE Books 
SET Description = 'Set in 1950s India, this novel follows Lakshmi, a henna artist in Jaipur, as she builds a new life after leaving an abusive marriage. A vibrant tale of female empowerment, art, and the complex social hierarchies of post-independence India.'
WHERE Title = 'The Henna Artist' AND Author = 'Alka Joshi';

UPDATE Books 
SET Description = 'Continuing the story from The Henna Artist, this novel follows Malik as he navigates life in the royal city of Jaipur. A coming-of-age story set against the backdrop of 1960s India, exploring themes of identity, loyalty, and belonging.'
WHERE Title = 'The Secret Keeper of Jaipur' AND Author = 'Alka Joshi';

UPDATE Books 
SET Description = 'The third book in the Jaipur trilogy follows Radha as she pursues her dreams in 1970s Paris, working for a prestigious perfume house. A sensual exploration of ambition, art, and the price of following your dreams in a foreign land.'
WHERE Title = 'The Perfumist of Paris' AND Author = 'Alka Joshi';

-- Update Kristen Loesch books
UPDATE Books 
SET Description = 'A multi-generational saga spanning from revolutionary Russia to modern-day England. When Rosie inherits a collection of Russian nesting dolls, she uncovers family secrets that stretch across decades of political upheaval and personal sacrifice.'
WHERE Title = 'The Last Russian Doll' AND Author = 'Kristen Loesch';

-- Update Anna-Marie McLemore books with magical realism/YA descriptions
UPDATE Books 
SET Description = 'A magical realism novel about Estrella, whose family has the gift of growing flowers from their skin, but each flower they create shortens their lifespan. When a boy named Bay arrives with his own dark magic, their worlds collide in dangerous and beautiful ways.'
WHERE Title = 'Wild Beauty' AND Author = 'Anna-Marie McLemore';

UPDATE Books 
SET Description = 'A lyrical novel about Sam, who was born a girl but lives as a boy, and Miel, who grows roses from her wrist. Set in a town where painted moons hang in the sky, this is a story about identity, acceptance, and the courage to be yourself.'
WHERE Title = 'When the Moon Was Ours' AND Author = 'Anna-Marie McLemore';

-- Update Malinda Lo books with LGBTQ+ YA/SF descriptions
UPDATE Books 
SET Description = 'A groundbreaking retelling of Cinderella featuring Lily, who discovers her attraction to her stepsister''s girlfriend Annie in 1950s California. A tender exploration of first love and coming of age in an era when being gay was both dangerous and illegal.'
WHERE Title = 'Last Night at the Telegraph Club' AND Author = 'Malinda Lo';

UPDATE Books 
SET Description = 'A reimagining of the Cinderella fairy tale featuring Ash, who finds love with the King''s huntress Kaisa instead of the prince. A beautifully written fantasy that subverts traditional fairy tale expectations.'
WHERE Title = 'Ash' AND Author = 'Malinda Lo';

UPDATE Books 
SET Description = 'A companion to Ash, following Kaisa and Taisin as they undertake a dangerous quest to save their kingdom. An epic fantasy featuring a lesbian romance and richly imagined world-building inspired by Asian mythology.'
WHERE Title = 'Huntress' AND Author = 'Malinda Lo';

-- Update Casey McQuiston book
UPDATE Books 
SET Description = 'A young adult romance following Chloe Green as she investigates the mysterious disappearance of her former best friend Shara Wheeler just before graduation. A story about small towns, secrets, and finding love in unexpected places.'
WHERE Title = 'I Kissed Shara Wheeler' AND Author = 'Casey McQuiston';

-- Update Kalynn Bayron books with fantasy descriptions
UPDATE Books 
SET Description = 'A bold YA fantasy reimagining where Cinderella is dead and Sophia must resist the oppressive patriarchal system that killed her. A powerful story about fighting injustice and claiming your own power in a world that wants to silence you.'
WHERE Title = 'Cinderella Is Dead' AND Author = 'Kalynn Bayron';

UPDATE Books 
SET Description = 'A contemporary fantasy following Briseis, who discovers she has inherited deadly power over plants. When she learns about her connection to ancient Greek mythology, she must master her abilities while protecting those she loves.'
WHERE Title = 'This Poison Heart' AND Author = 'Kalynn Bayron';

-- Add generic descriptions for books that don't have specific ones
UPDATE Books 
SET Description = CASE 
    WHEN Genre = 'Romance' THEN 'A heartwarming romance novel that explores the complexities of love and relationships. Follow compelling characters as they navigate the challenges of finding and maintaining love in this engaging contemporary story.'
    WHEN Genre = 'Historical Fiction' THEN 'A richly detailed historical fiction novel that brings the past to life through compelling characters and authentic period details. Experience history through the eyes of those who lived it in this immersive narrative.'
    WHEN Genre = 'Fiction' THEN 'A thought-provoking work of contemporary fiction that explores the human condition with nuance and depth. Through compelling characters and engaging prose, this novel examines life, relationships, and personal growth.'
    WHEN Genre = 'Fantasy' THEN 'An imaginative fantasy novel that transports readers to a world of magic and adventure. With rich world-building and compelling characters, this story explores themes of heroism, destiny, and the power of imagination.'
    WHEN Genre = 'Young Adult' THEN 'A compelling young adult novel that addresses the challenges and triumphs of growing up. With authentic characters and relevant themes, this story speaks to the universal experience of coming of age.'
    WHEN Genre = 'Science Fiction' THEN 'A thought-provoking science fiction novel that explores the possibilities and consequences of technological advancement. Set in a carefully crafted future world, this story examines humanity through the lens of speculative fiction.'
    WHEN Genre = 'Gothic' THEN 'A atmospheric Gothic novel that combines elements of horror, romance, and mystery. With its dark mood and supernatural elements, this story creates a haunting and unforgettable reading experience.'
    WHEN Genre = 'Magical Realism' THEN 'A beautifully crafted work of magical realism that blends the everyday with the extraordinary. Through lyrical prose and fantastical elements, this novel explores deeper truths about life and human nature.'
    ELSE 'A captivating novel that offers readers an engaging literary experience. Through skillful storytelling and well-developed characters, this book provides entertainment while exploring meaningful themes and universal human experiences.'
END
WHERE Description IS NULL OR Description = '';

PRINT 'Book descriptions have been successfully added to all books in the database.';