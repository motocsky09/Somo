Plasează aici fișierele de font pentru proiect.

Am mutat fișierele în `src/assets/fonts/goga/` și `src/styles.css` folosește acum acea locație.

Fișierele găsite și folosite (ex: OTF):
- GogaTest-Thin-*.otf -> font-weight: 100
- GogaTest-Extralight-*.otf -> 200
- GogaTest-Light-*.otf -> 300
- GogaTest-Regular-*.otf -> 400
- GogaTest-Medium-*.otf -> 500 (folosit pentru `font-weight: 500`)
- GogaTest-Semibold-*.otf -> 600
- GogaTest-Bold-*.otf -> 700
- GogaTest-Extrabold-*.otf -> 800
- GogaTest-Black-*.otf -> 900

Recomandare optimizare:
- Convertiți OTF în `woff2` pentru performanță. Pot face conversia local dacă îmi dai OK (instalez un mic utilitar sau folosesc `npm`), sau o faci offline cu unelte precum `woff2_compress`.
- După conversie, înlocuiți URL-urile din `src/styles.css` cu fișierele `.woff2` pentru încărcare mai rapidă.

Dacă preferi să păstrăm OTF, acestea sunt compatibile cu majoritatea browserelor moderne, dar `woff2` rămâne cea mai optimă alegere.
