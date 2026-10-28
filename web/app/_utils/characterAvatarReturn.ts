import { dk, dl, dw, elf, gl, mg, rf, rw, sl, sum } from "../ranking/(tables)/images";

function getImage(key: string) {
  switch (key) {
    case "00000040-0003-0000-0000-000000000000": return dw;
    case "00000040-0007-0000-0000-000000000000": return dk;
    case "00000040-000b-0000-0000-000000000000": return elf;
    case "00000040-000d-0000-0000-000000000000": return mg;
    case "00000040-0011-0000-0000-000000000000": return dl;
    case "00000040-0017-0000-0000-000000000000": return sum;
    case "00000040-0019-0000-0000-000000000000": return rf;
    case "00000040-0002-0000-0000-000000000000": return dw;
    case "00000040-0006-0000-0000-000000000000": return dk;
    case "00000040-000a-0000-0000-000000000000": return elf;
    case "00000040-000c-0000-0000-000000000000": return mg;
    case "00000040-0010-0000-0000-000000000000": return dl;
    case "00000040-0016-0000-0000-000000000000": return sum;
    case "00000040-0018-0000-0000-000000000000": return rf;
    case "00000040-0000-0000-0000-000000000000": return dw;
    case "00000040-0004-0000-0000-000000000000": return dk;
    case "00000040-0008-0000-0000-000000000000": return elf;
    case "00000040-0014-0000-0000-000000000000": return sum;
    default: return dk;
  }
}

function getClassName(key: string): string {
  switch (key) {
    case "00000040-0003-0000-0000-000000000000": return "Grand Master";
    case "00000040-0007-0000-0000-000000000000": return "Blade Master";
    case "00000040-000b-0000-0000-000000000000": return "High Elf";
    case "00000040-000d-0000-0000-000000000000": return "Duel Master";
    case "00000040-0011-0000-0000-000000000000": return "Lord Emperor";
    case "00000040-0017-0000-0000-000000000000": return "Dimension Master";
    case "00000040-0019-0000-0000-000000000000": return "Fist Master";
    case "00000040-0002-0000-0000-000000000000": return "Soul Master";
    case "00000040-0006-0000-0000-000000000000": return "Blade Knight";
    case "00000040-000a-0000-0000-000000000000": return "Muse Elf";
    case "00000040-000c-0000-0000-000000000000": return "Magic Gladiator";
    case "00000040-0010-0000-0000-000000000000": return "Dark Lord";
    case "00000040-0016-0000-0000-000000000000": return "Bloody Summoner";
    case "00000040-0018-0000-0000-000000000000": return "Rage Fighter";
    case "00000040-0000-0000-0000-000000000000": return "Dark Wizard";
    case "00000040-0004-0000-0000-000000000000": return "Dark Knight";
    case "00000040-0008-0000-0000-000000000000": return "Fairy Elf";
    case "00000040-0014-0000-0000-000000000000": return "Summoner";
    default: return "Adventurer";
  }
}

export { getImage, getClassName };
