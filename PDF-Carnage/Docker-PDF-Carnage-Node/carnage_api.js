const express = require('express')
const cors = require('cors');
const app = express()
const fs = require('fs')
const { SitemapStream, streamToPromise } = require('sitemap')
const { createGzip } = require('zlib')
const { Readable } = require('stream')
const port = 3000
app.use(cors())
let sitemap

app.listen(port, () => {
	console.log(`App listening at http://localhost:${port}`);
})

app.get("/", (req, res) => {
	res.send("PDF Carnage API");
})

// https://www.npmjs.com/package/sitemap
app.get('/sitemap.xml', function(req, res) {
  res.header('Content-Type', 'application/xml');
  res.header('Content-Encoding', 'gzip');

  // if we have a cached entry send it
  if (sitemap) {
    res.send(sitemap)
    return
  }

  try {
    const smStream = new SitemapStream({ hostname: 'http://nodeapi:3000/' })
    const pipeline = smStream.pipe(createGzip())

    // pipe your entries or directly write them.
    smStream.write({ url: '/api/info' })
    smStream.write({ url: '/api/flag' })

    // cache the response
    streamToPromise(pipeline).then(sm => sitemap = sm)
    // make sure to attach a write stream such as streamToPromise before ending
    smStream.end()
    // stream write the response
    pipeline.pipe(res).on('error', (e) => {throw e})
  } catch (e) {
    console.error(e)
    res.status(500).end()
  }
})

app.get("/api/info", (req, res) => {
	res.send("PDF Carnage API is running");
})

app.get("/api/flag", (req, res) => {
	const ip = req.connection.remoteAddress;
	fs.writeFile("/tmp/ip.txt", ip, err => {
		if (err) {
			console.error(err);
		}
	});
	if(ip === "127.0.0.1" || ip === "::1" || ip === "::ffff:127.0.0.1" || ip === "172.16.5.55" || ip === "::ffff:172.16.5.55") {
		res.send("CSR{PdF_G3nER4t0r5_C4n_B3_D4ng3R0Us}");
	} else {
		res.send("The call is not coming from inside the house")
	}
})
