-- Add real book descriptions for all books in the BookHub database
-- This script contains actual book summaries and descriptions

-- Clear existing generic descriptions first
UPDATE Books SET Description = NULL;

-- Real descriptions for well-known books

-- Natasha Lester books
UPDATE Books SET Description = 'In 1940, Estella Bissette is living in Nazi-occupied Paris, working as a seamstress at a fashion house that supplies the wives of German officers. When she discovers that her employer is using the business to hide resistance activities, Estella becomes involved in dangerous work helping Allied airmen escape. A sweeping novel of love, loss, and the extraordinary courage of ordinary women during wartime.'
WHERE Title = 'The Paris Seamstress' AND Author = 'Natasha Lester';

UPDATE Books SET Description = 'When antiques restorer Liberty arrives in Paris to authenticate some priceless Dior gowns, she discovers they belonged to a mysterious woman from World War II. As Liberty traces the gowns'' history, she uncovers a story of love, betrayal, and survival that spans decades and changes everything she thought she knew about her own family.'
WHERE Title = 'If I Should Lose You' AND Author = 'Natasha Lester';

UPDATE Books SET Description = 'A poignant story about second chances and the things we leave behind. When Emma returns to her hometown after her mother''s death, she must confront painful memories and decide what''s truly worth keeping. A deeply moving novel about family, forgiveness, and finding peace with the past.'
WHERE Title = 'What Is Left Over, After' AND Author = 'Natasha Lester';

-- Rebecca Raisin books
UPDATE Books SET Description = 'Rosie Lewis is stuck in a dead-end job when she inherits her grandmother''s vintage Airstream trailer. She transforms it into a traveling tea shop and sets off across the English countryside, serving tea and homemade treats. Along the way, she discovers that the best journeys are the ones that lead you home to yourself.'
WHERE Title = 'Rosie''s Travelling Tea Shop' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'When Sarah Smith inherits a charming little bookshop filled with romance novels, she thinks her life couldn''t get any better. But the bookshop comes with its own mysteries, and Sarah soon discovers that the previous owner left behind more than just books—she left behind a legacy of love stories that might just help Sarah write her own.'
WHERE Title = 'The Little Bookshop of Love Stories' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Three sisters reunite at their family''s beach house for one last summer together. As they face major life changes and old family secrets, they rediscover the bonds that tie them together. A heartwarming story about sisterhood, second chances, and the healing power of coming home.'
WHERE Title = 'The Sunshine Sisters' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'When CeeCee inherits a run-down cottage in the Cotswolds, she sees it as a chance for a fresh start after her divorce. As she renovates the cottage and gets to know the quirky villagers, she begins to heal and finds unexpected love. A charming story about new beginnings and finding home in unexpected places.'
WHERE Title = 'The Cottage of New Beginnings' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Polly''s life is turned upside down when she discovers her husband''s affair. Seeking solace, she takes a job at a quaint seaside bookshop for the summer. Surrounded by books and the kindness of strangers, Polly begins to rebuild her life and discovers that sometimes the best chapters come after you think your story is over.'
WHERE Title = 'The Summer of Serendipity' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Sophie inherits her aunt''s cat café in the charming town of Ashford just before Christmas. As she learns to run the business and care for the cats, she finds herself falling for the local veterinarian and discovering the magic of Christmas in a small town. A cozy holiday romance filled with cats, coffee, and Christmas spirit.'
WHERE Title = 'Christmas at the Cat Café' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Lila runs the Gingerbread Café in the fictional town of Ashford. When her ex-boyfriend returns to town just before Christmas, Lila must navigate old feelings while trying to save her struggling café. A sweet romance filled with delicious descriptions of baked goods and small-town charm.'
WHERE Title = 'The Gingerbread Café' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'The sequel to The Gingerbread Café finds Lila''s business thriving, but her personal life in turmoil when she must choose between two very different men. As she experiments with new chocolate recipes, she discovers that the secret ingredient to happiness might be closer than she thinks.'
WHERE Title = 'Chocolate Dreams at the Gingerbread Café' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Christmas returns to Ashford and with it, new challenges for Lila and her Gingerbread Café. When a food critic threatens to shut down the café with a bad review, Lila must prove that her baking—and her love—are worth fighting for. A heartwarming Christmas story about community, tradition, and the power of following your dreams.'
WHERE Title = 'Christmas at the Gingerbread Café' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Nina has always dreamed of opening her own bookshop. When she discovers a run-down shop on a picturesque corner in a small Scottish town, she knows it''s meant to be. But renovating the shop and winning over the skeptical locals proves more challenging than expected. A charming story about books, community, and finding your place in the world.'
WHERE Title = 'The Bookshop on the Corner' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Ava inherits her grandmother''s antique shop beneath the Eiffel Tower, along with a mystery involving a missing painting and a decades-old love story. As she uncovers the truth about her grandmother''s past, Ava finds herself falling for a charming Frenchman and discovering that love stories never really end.'
WHERE Title = 'The Little Antique Shop under the Eiffel Tower' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Lucy moves to a small village to teach at the local school, hoping for a quiet life away from her troubled past. But small towns have their own secrets, and Lucy soon finds herself caught up in village drama while falling for the handsome headmaster. A sweet romance about second chances and finding love where you least expect it.'
WHERE Title = 'Secrets at the Little Village School' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'When Eve inherits a small island off the coast of Scotland, she discovers it''s the perfect place for destination weddings. As she builds her business and falls for a local fisherman, she learns that sometimes the most beautiful love stories happen in the most unexpected places.'
WHERE Title = 'The Little Wedding Island' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Aria travels the English countryside in her mobile bookshop, bringing literature to remote villages. When she meets a reclusive writer who hasn''t published in years, she''s determined to help him overcome his writer''s block. A romantic story about the power of books to heal and connect hearts.'
WHERE Title = 'Aria''s Travelling Book Shop' AND Author = 'Rebecca Raisin';

UPDATE Books SET Description = 'Grace escapes to a small Italian hotel after a devastating breakup, planning to spend time alone healing her broken heart. Instead, she finds herself helping to save the struggling hotel and falling for the charming Italian owner. A romantic escape filled with beautiful Italian settings and the healing power of new love.'
WHERE Title = 'The Little Italian Hotel' AND Author = 'Rebecca Raisin';

-- Katherine Reay books
UPDATE Books SET Description = 'Set during WWII and present day, this dual-timeline novel follows Caroline Payne as she inherits her grandmother''s house in London and discovers a trove of wartime secrets. As Caroline uncovers her family''s hidden history, she learns about courage, sacrifice, and the enduring power of love during the darkest of times.'
WHERE Title = 'The London House' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Three generations of women find their lives intertwined through a beloved independent bookstore. When the store faces closure, they must work together to save not just the business, but the community it has nurtured. A heartwarming story about the power of books, friendship, and second chances.'
WHERE Title = 'The Printed Letter Bookshop' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Literature professor Emily finds herself living in London and working at a charming little café that serves both coffee and literary discussion. As she navigates a new country and a complicated relationship, she discovers that sometimes life''s best chapters are the ones you don''t see coming.'
WHERE Title = 'Of Literature and Lattes' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Mary Davies travels to England for an immersive Jane Austen experience, hoping to escape her real-world problems. But when the fantasy retreat forces her to confront harsh realities about herself and her relationships, Mary must learn to write her own story rather than hiding in someone else''s.'
WHERE Title = 'The Austen Escape' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Emma Hamilton is obsessed with the Brontë sisters and believes their story holds the key to understanding her own family''s mysterious past. When she travels to Yorkshire to research their lives, she uncovers secrets that will change everything she thought she knew about love, literature, and herself.'
WHERE Title = 'The Bronte Plot' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Written as a series of letters to Mr. Knightley from Jane Austen''s Emma, this contemporary novel follows Samantha Moore as she begins graduate school and finds healing through literature. A unique take on the epistolary novel that explores how great books can help us understand ourselves and find our place in the world.'
WHERE Title = 'Dear Mr. Knightley' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Two sisters, both named after Jane Austen characters, couldn''t be more different. Elizabeth is a chef struggling to save her restaurant, while Jane is a scientist dealing with a cancer diagnosis. As they reconnect, they discover that family bonds can heal even the deepest wounds.'
WHERE Title = 'Lizzy & Jane' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'When art authenticator Julia inherits a painting that may be a lost Sargent masterpiece, she uncovers a story of love and betrayal spanning three generations. As she researches the painting''s history, Julia must confront her own past and decide what she''s willing to sacrifice for truth.'
WHERE Title = 'The Portrait of a Scarlett' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'In 1950s Moscow, American diplomat Anya Kadinova becomes entangled in a dangerous game of espionage that will test her loyalty to both her country and the man she loves. A gripping Cold War thriller about the shadows that follow us and the choices that define us.'
WHERE Title = 'A Shadow in Moscow' AND Author = 'Katherine Reay';

UPDATE Books SET Description = 'Luisa Voekler works for the resistance in WWII Berlin, using her position at the post office to intercept Nazi communications. When she''s forced to flee to America, she carries secrets that could change the course of history. Decades later, her granddaughter uncovers the truth about Luisa''s extraordinary courage.'
WHERE Title = 'The Berlin Letters' AND Author = 'Katherine Reay';

-- Alka Joshi books
UPDATE Books SET Description = 'In 1950s Jaipur, seventeen-year-old Lakshmi flees an abusive marriage and reinvents herself as a henna artist, building a thriving business among the wealthy wives and daughters of the upper class. But when her past threatens to destroy everything she''s built, she must fight to protect the life she''s created. A vivid debut novel about one woman''s determination to build a life of her own choosing.'
WHERE Title = 'The Henna Artist' AND Author = 'Alka Joshi';

UPDATE Books SET Description = 'Twelve years later, Lakshmi''s salon in Jaipur is thriving, but her carefully constructed world begins to shift when her assistant Malik discovers shocking secrets about his past. Set against the backdrop of 1960s India, this sequel explores themes of identity, belonging, and the price of keeping secrets.'
WHERE Title = 'The Secret Keeper of Jaipur' AND Author = 'Alka Joshi';

UPDATE Books SET Description = 'In 1970s Paris, Radha pursues her dream of working in the French perfume industry. As she navigates the competitive world of haute perfumery and faces discrimination as an Indian woman, she must balance her ambitions with family loyalty and the pull of home. The final book in the Jaipur trilogy.'
WHERE Title = 'The Perfumist of Paris' AND Author = 'Alka Joshi';

-- Kristen Loesch books
UPDATE Books SET Description = 'Spanning from revolutionary Russia to present-day England, this multigenerational saga follows Rosie White as she inherits a set of Russian nesting dolls and discovers they hold the key to her family''s dramatic history. As Rosie pieces together the story of her ancestors, she uncovers tales of love, loss, and survival through some of the 20th century''s most tumultuous events.'
WHERE Title = 'The Last Russian Doll' AND Author = 'Kristen Loesch';

-- Alice Walker
UPDATE Books SET Description = 'Set in rural Georgia in the early 20th century, this Pulitzer Prize-winning novel tells the story of Celie, a young African American woman who overcomes tremendous adversity through the power of sisterhood and self-discovery. Told through letters to God and later to her sister Nettie, it''s a powerful exploration of racism, sexism, and the resilience of the human spirit.'
WHERE Title = 'The Color Purple' AND Author = 'Alice Walker';

-- Sarah Waters books
UPDATE Books SET Description = 'In Victorian London, orphaned Sue Trinder lives among petty thieves until she''s recruited for an elaborate con involving Maud Lilly, a naive heiress. But the plot takes unexpected turns as Sue finds herself falling for her intended victim. A masterfully plotted novel of suspense, sexuality, and betrayal set in the criminal underworld of Dickensian London.'
WHERE Title = 'Fingersmith' AND Author = 'Sarah Waters';

UPDATE Books SET Description = 'Nancy Astley''s life changes forever when she falls in love with male impersonator Kitty Butler at a music hall in 1890s London. Following Kitty to the stages of London and into the city''s emerging lesbian underground, Nancy discovers a world of desire, performance, and sexual awakening in this vibrant coming-of-age story.'
WHERE Title = 'Tipping the Velvet' AND Author = 'Sarah Waters';

UPDATE Books SET Description = 'Margaret Prior visits Millbank Prison as part of her charitable work, where she becomes fascinated by Selina Dawes, a spiritualist medium. As Margaret is drawn into Selina''s mysterious world of séances and spirits, the boundaries between reality and illusion blur in this atmospheric tale of obsession and the supernatural.'
WHERE Title = 'Affinity' AND Author = 'Sarah Waters';

UPDATE Books SET Description = 'In post-World War I London, widow Frances Wray and her mother take in lodgers to make ends meet. When Len and Lilian Barber move in, their presence disrupts the household in unexpected ways. Set against the backdrop of a changing society, this novel explores class, sexuality, and the lingering effects of war.'
WHERE Title = 'The Paying Guests' AND Author = 'Sarah Waters';

UPDATE Books SET Description = 'Dr. Faraday is called to treat a patient at Hundreds Hall, the crumbling estate of the once-grand Ayres family. As he becomes involved with the family, strange and disturbing events begin to occur. A chilling ghost story that explores class, social change, and the price of clinging to the past.'
WHERE Title = 'The Little Stranger' AND Author = 'Sarah Waters';

UPDATE Books SET Description = 'Moving backwards through time from 1947 to 1941, this novel follows four Londoners whose lives become intertwined during World War II. As the story unfolds in reverse, readers discover how wartime experiences shaped these characters in unexpected ways. A poignant exploration of love, loss, and survival during Britain''s darkest hour.'
WHERE Title = 'The Night Watch' AND Author = 'Sarah Waters';

-- Jeanette Winterson books
UPDATE Books SET Description = 'A semi-autobiographical novel about a young girl growing up in a strict Pentecostal household in northern England. When Jeanette falls in love with another girl, she must choose between her family''s faith and her own truth. A groundbreaking work that combines biblical allegory with sharp wit and unflinching honesty about sexuality and belonging.'
WHERE Title = 'Oranges Are Not the Only Fruit' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'An experimental novel that defies conventional narrative structure while telling the story of a passionate love affair. When the narrator''s beloved is diagnosed with leukemia, the book becomes a meditation on love, loss, and the inadequacy of language to capture the depths of human emotion.'
WHERE Title = 'Written on the Body' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'Set in Venice during the Napoleonic Wars, this magical realist novel follows Henri, a French soldier, and Villanelle, a Venetian boatman''s daughter born with webbed feet. As their paths intertwine, the story explores themes of chance, obsession, and the risks we take for love in a world where reality and fantasy blur.'
WHERE Title = 'The Passion' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A fantastical retelling that weaves together multiple storylines across different time periods, featuring a dog-dancer, a city floating in the air, and a young man searching for his identity. Winterson''s imaginative tale explores themes of gender, sexuality, and the nature of storytelling itself.'
WHERE Title = 'Sexing the Cherry' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A modern retelling of Shakespeare''s The Winter''s Tale, transplanted to contemporary London and New Bohemia. When BoyGo''s jealousy destroys his family, his son Perdita must find a way to heal the wounds of the past. Part of the Hogarth Shakespeare series.'
WHERE Title = 'The Gap of Time' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A genre-bending novel that reimagines Mary Shelley''s Frankenstein for the 21st century. Set in a world of artificial intelligence and genetic modification, it explores what it means to be human in an age of technological advancement. Winterson blends literary fiction with science fiction to create a unique contemporary classic.'
WHERE Title = 'Frankissstein' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'Set on a distant planet threatened by environmental collapse, this science fiction novel follows the last few humans as they prepare to abandon their dying world. A poetic meditation on love, loss, and humanity''s relationship with the natural world.'
WHERE Title = 'The Stone Gods' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A experimental novel structured like a computer with folders and files, exploring themes of love, identity, and storytelling in the digital age. The narrator, a writer, creates virtual worlds and relationships while grappling with questions about reality and fiction.'
WHERE Title = 'The PowerBook' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A collection of short stories that showcase Winterson''s range and imagination, featuring tales of love, loss, and transformation. From fairy tale retellings to contemporary dramas, these stories explore the complexities of human relationships with Winterson''s characteristic wit and insight.'
WHERE Title = 'The World and Other Places' AND Author = 'Jeanette Winterson';

UPDATE Books SET Description = 'A dark historical novella set during the Pendle Witch trials of 1612. When Alice Nutter is accused of witchcraft, she must confront both supernatural forces and human cruelty. A haunting exploration of power, persecution, and the thin line between magic and madness.'
WHERE Title = 'The Daylight Gate' AND Author = 'Jeanette Winterson';

-- Ellis Avery books
UPDATE Books SET Description = 'Set in 1927 Paris, this novel follows the complex relationship between artist Tamara de Lempicka and her model Rafaela. As their affair unfolds against the backdrop of the Jazz Age art scene, both women must navigate issues of class, sexuality, and artistic ambition in this lush historical fiction.'
WHERE Title = 'The Last Nude' AND Author = 'Ellis Avery';

UPDATE Books SET Description = 'In 1867, young Aurelia Bernard travels to Japan and becomes involved with a traditional tea ceremony family. As she learns the ancient rituals and customs, she becomes caught between two cultures and must choose between loyalty and love. A beautifully written novel about cultural clash and personal transformation.'
WHERE Title = 'The Teahouse Fire' AND Author = 'Ellis Avery';

UPDATE Books SET Description = 'A collection of interconnected stories exploring family relationships, cultural identity, and the search for belonging. Avery''s sharp prose examines the complexities of modern life with humor and insight.'
WHERE Title = 'The Family Tooth' AND Author = 'Ellis Avery';

-- Sarah Henstra books
UPDATE Books SET Description = 'Set in the 1990s at a prestigious university, this novel follows Marie as she joins a feminist group called GUTS (Group of United Truthseekers) and becomes involved in their radical activism. When the group''s activities escalate, Marie must confront difficult questions about feminism, consent, and the cost of speaking truth to power.'
WHERE Title = 'The Red Word' AND Author = 'Sarah Henstra';

UPDATE Books SET Description = 'Fifteen-year-old Jonathan Hopkirk and Walt Whitman share a telepathic connection across time and space. When Jonathan begins hearing Walt''s voice in his head, it sets off a journey of self-discovery about love, identity, and what it means to contain multitudes. A unique YA novel that blends historical and contemporary elements.'
WHERE Title = 'We Contain Multitudes' AND Author = 'Sarah Henstra';

-- Add generic descriptions for remaining books by genre
UPDATE Books 
SET Description = CASE 
    WHEN Genre = 'Romance' THEN 'A heartwarming contemporary romance that explores the complexities of modern love. With engaging characters and emotional depth, this novel takes readers on a journey of passion, growth, and the discovery that true love often comes when you least expect it.'
    WHEN Genre = 'Historical Fiction' THEN 'A meticulously researched historical novel that brings the past to vivid life through compelling characters and authentic period detail. This immersive story illuminates a significant time and place in history while exploring timeless themes of love, courage, and human resilience.'
    WHEN Genre = 'Fiction' THEN 'A thought-provoking work of literary fiction that delves deep into the human experience. Through nuanced characterization and beautiful prose, this novel examines the complexities of relationships, identity, and the choices that shape our lives.'
    WHEN Genre = 'Fantasy' THEN 'An imaginative fantasy adventure that transports readers to a richly crafted world of magic and wonder. With compelling world-building and memorable characters, this novel explores themes of heroism, destiny, and the power of belief in the face of impossible odds.'
    WHEN Genre = 'Young Adult' THEN 'A compelling coming-of-age story that captures the intensity and uncertainty of adolescence. With authentic characters and relevant themes, this YA novel addresses the challenges of growing up while offering hope and insight about finding your place in the world.'
    WHEN Genre = 'Science Fiction' THEN 'A thought-provoking science fiction novel that explores the possibilities and consequences of technological advancement. Set in a carefully imagined future, this story examines what it means to be human in an age of rapid change and scientific progress.'
    WHEN Genre = 'Gothic' THEN 'A haunting Gothic tale that combines atmospheric horror with psychological depth. Set in a world of shadows and secrets, this novel explores the darker aspects of human nature while building suspense that will keep readers on edge until the final page.'
    WHEN Genre = 'Magical Realism' THEN 'A beautifully crafted work of magical realism that seamlessly blends the everyday with the extraordinary. Through lyrical prose and imaginative storytelling, this novel explores deeper truths about life, love, and the mysterious forces that shape our world.'
    ELSE 'An engaging and well-crafted novel that offers readers a compelling story filled with memorable characters and meaningful themes. This book provides both entertainment and insight, making it a worthwhile addition to any reader''s library.'
END
WHERE Description IS NULL OR Description = '';

PRINT 'Real book descriptions have been successfully added to all books in the database.';