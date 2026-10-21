#!/usr/bin/env python3
"""Repo sanity checks run before every build. Fails loudly; never pretends."""
import json, re, sys, pathlib, subprocess
root = pathlib.Path(__file__).resolve().parent.parent
errs = []
def err(m): errs.append(m)

v = json.loads((root/"version.json").read_text())
if v.get("packageId") != "com.timegate.timetravel": err("packageId must be com.timegate.timetravel")
if not isinstance(v.get("versionCode"), int) or v["versionCode"] < 1: err("versionCode must be int >= 1")
# versionCode must increase vs last release tag, if tags exist
try:
    tags = subprocess.run(["git","tag","--list","v*"],cwd=root,capture_output=True,text=True).stdout.split()
    for t in tags:
        out = subprocess.run(["git","show",f"{t}:version.json"],cwd=root,capture_output=True,text=True)
        if out.returncode==0:
            old = json.loads(out.stdout)["versionCode"]
            head_is_tag = subprocess.run(["git","describe","--tags","--exact-match"],cwd=root,capture_output=True,text=True).stdout.strip()==t
            if v["versionCode"] < old or (v["versionCode"]==old and not head_is_tag): err(f"versionCode {v['versionCode']} must exceed {old} from tag {t}")
except Exception as e: print("note: tag check skipped:", e)

builders = set(re.findall(r'\["(\w+)"\]\s*=\s*\w+\.Build', (root/"Assets/Scripts/World/WorldFactory.cs").read_text()))
siteids = set()
for p in sorted((root/"Assets/Resources/Packs").glob("*.json")):
    d = json.loads(p.read_text(encoding="utf-8"))
    for k in ("id","packVersion","countryCode","country","civilization","status","sites"):
        if k not in d: err(f"{p.name}: missing {k}")
    for s in d.get("sites",[]):
        if s["id"] in siteids: err(f"duplicate site id {s['id']}")
        siteids.add(s["id"])
        if d["status"]=="playable" and s.get("playable"):
            if s.get("worldBuilder") not in builders: err(f"{p.name}:{s['id']} playable but world builder '{s.get('worldBuilder')}' not registered")
            if not s.get("references"): err(f"{p.name}:{s['id']} playable site without references")
        for c in s.get("characters",[]):
            if not c.get("fallback") or not c.get("topics"): err(f"character {c['id']} needs fallback + topics")
            for tp in c["topics"]:
                if not tp.get("keywords") or not tp["answer"].get("ar") or not tp["answer"].get("en"): err(f"character {c['id']} topic incomplete")
        poi_ids={x["id"] for x in s.get("pois",[])}
        for q in s.get("quests",[]):
            for o in q["objectivePoiIds"]:
                if o not in poi_ids: err(f"quest {q['id']} references unknown POI {o}")
json.loads((root/"Packages/manifest.json").read_text())

# secrets / forbidden files
bad = ["*.keystore","*.jks","google-services.json","GoogleService-Info.plist",".env"]
for pat in bad:
    for f in root.rglob(pat):
        if ".git" not in f.parts: err(f"forbidden file committed: {f.relative_to(root)}")
pat = re.compile(r'(service_role|AIza[0-9A-Za-z_\-]{30,}|sk-[A-Za-z0-9]{20,}|-----BEGIN [A-Z ]*PRIVATE KEY)')
for f in list(root.rglob("*.cs"))+list(root.rglob("*.json"))+list(root.rglob("*.md"))+list(root.rglob("*.sql"))+list(root.rglob("*.yml")):
    if ".git" in f.parts: continue
    for i,l in enumerate(f.read_text(encoding="utf-8",errors="ignore").splitlines(),1):
        if pat.search(l) and "never" not in l.lower() and "no " not in l.lower(): err(f"possible secret {f.relative_to(root)}:{i}")

# crude C# structural check
for f in (root/"Assets").rglob("*.cs"):
    t = re.sub(r'"(\\.|[^"\\])*"|//[^\n]*', '', f.read_text(encoding="utf-8"))
    for a,b in ("{}","()","[]"):
        if t.count(a)!=t.count(b): err(f"{f.relative_to(root)}: unbalanced {a}{b} ({t.count(a)} vs {t.count(b)})")

if errs:
    print("VALIDATION FAILED"); [print(" -",e) for e in errs]; sys.exit(1)
print(f"validation OK  (packs: {len(siteids)} sites, builders: {sorted(builders)}, version {v['versionName']}+{v['versionCode']})")
