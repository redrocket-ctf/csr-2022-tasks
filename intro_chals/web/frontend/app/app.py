from datetime import datetime, timedelta
import subprocess

from flask import Flask, redirect, render_template, request

app = Flask(__name__)

last_visit = None

@app.route("/", methods = ["GET"])
def index():
    return render_template("index.tpl", msg=request.args.get("msg", ""))

@app.route("/visit", methods = ["POST"])
def visit():
    token = request.form["token"]

    if not token:
        return redirect("/?msg=No+token+given!")

    global last_visit
    if last_visit and datetime.utcnow() - last_visit < timedelta(seconds=35):
        return redirect("/?msg=The+admin+can't+type+this+fast!")

    last_visit = datetime.utcnow()
    res = subprocess.run(["python3", "browser.py", token])
    try:
        res.check_returncode()
    except subprocess.CalledProcessError:
        return redirect("/?msg=The+admin+didn't+like+your+token!")

    return redirect("/")

if __name__ == "__main__":
    app.run("0.0.0.0", 8080)
