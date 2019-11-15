CREATE TABLE `request` (
  `dateOfRequest` datetime NOT NULL,
  `user` varchar(20) COLLATE utf8_unicode_ci NOT NULL,
  `txt` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  `trans` varchar(255) COLLATE utf8_unicode_ci NOT NULL,
  PRIMARY KEY (`dateOfRequest`),
  KEY `request_user` (`user`),
  CONSTRAINT `request_user` FOREIGN KEY (`user`) REFERENCES `user` (`login`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_unicode_ci