-- Authorized one-time custom test-stat increase. Run only with the game server stopped,
-- after backing up the dedicated originsix-local database. Baseline guards prevent repeats.
\set ON_ERROR_STOP on
BEGIN;

CREATE TEMP TABLE requested_stats (name text, attribute text, expected real, PRIMARY KEY(name, attribute)) ON COMMIT DROP;
INSERT INTO requested_stats VALUES
  ('RKINGG', 'Base Strength', 697),
  ('RKINGG', 'Base Agility', 700),
  ('RKINGG', 'Base Vitality', 1283),
  ('RKINGG', 'Base Energy', 404),
  ('OpenCodeX', 'Base Strength', 411),
  ('OpenCodeX', 'Base Agility', 300),
  ('OpenCodeX', 'Base Vitality', 200),
  ('OpenCodeX', 'Base Energy', 1554),
  ('OpenCodeXx', 'Base Strength', 1305),
  ('OpenCodeXx', 'Base Agility', 860),
  ('OpenCodeXx', 'Base Vitality', 200),
  ('OpenCodeXx', 'Base Energy', 100);

CREATE TEMP TABLE before_stats ON COMMIT DROP AS
SELECT s."Id" AS id, c."Name" AS name, a."Designation" AS attribute, s."Value" AS value
FROM data."Account" ac
JOIN data."Character" c ON c."AccountId" = ac."Id"
JOIN data."StatAttribute" s ON s."CharacterId" = c."Id"
JOIN config."AttributeDefinition" a ON a."Id" = s."DefinitionId"
JOIN requested_stats r ON r.name = c."Name" AND r.attribute = a."Designation"
WHERE ac."LoginName" = 'rkingg';

DO $$
BEGIN
  IF current_database() <> 'openmu' THEN RAISE EXCEPTION 'Wrong database'; END IF;
  IF (SELECT count(*) FROM before_stats) <> 12 THEN RAISE EXCEPTION 'Expected exactly 12 target attributes'; END IF;
  IF EXISTS (
    SELECT 1 FROM before_stats b JOIN requested_stats r USING(name, attribute)
    WHERE b.value <> r.expected OR b.value + 5000 > 65535
  ) THEN RAISE EXCEPTION 'Baseline changed or increase already applied; review before retrying'; END IF;
END $$;

UPDATE data."StatAttribute" s
SET "Value" = b.value + 5000
FROM before_stats b WHERE s."Id" = b.id;

DO $$
BEGIN
  IF EXISTS (
    SELECT 1 FROM before_stats b JOIN data."StatAttribute" s ON s."Id" = b.id
    WHERE s."Value" - b.value <> 5000
  ) THEN RAISE EXCEPTION 'Saved delta is not exactly +5000'; END IF;
END $$;

SELECT b.name, b.attribute, b.value AS before, s."Value" AS after, s."Value"-b.value AS delta
FROM before_stats b JOIN data."StatAttribute" s ON s."Id" = b.id
ORDER BY b.name, b.attribute;
COMMIT;
