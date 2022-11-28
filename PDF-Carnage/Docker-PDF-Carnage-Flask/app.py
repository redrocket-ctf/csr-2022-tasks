from flask import Flask, request, send_file, redirect, url_for, escape, after_this_request
from flask_limiter import Limiter
from flask_limiter.util import get_remote_address
import pdfkit
import os
import requests
import uuid
app = Flask(__name__)
#api = 'http://localhost:3000/api'
dangerous_input = ['script', '<', '>', '/', '\\', 'img', 'input', '=', '!', '\'', '"']

limiter = Limiter(
        app,
        key_func=get_remote_address,
        default_limits=['50 per minute']
        )


@app.route('/', methods=['GET'])
@limiter.limit('25 per minute')
def home():
#    try:
#        resp = requests.get(f'{api}/info')
#    except:
#        return """Can't connect to Carnage API. Is it up and running?"""

    return """
    <html>	
        <head>
            <title>Welcome to PDF Carnage</title>
        </head>

        <div id="app">
            <h3>Generate your PDF here</h3>
        </div>
            <p id="name"></p>
        <form action="/pdf" method="post" >
            <input id="text" type="input" name="pdf_form"/></br></br>
            <input type="submit"/>
        </form>
    </html>
    """


@app.route('/pdf', methods=['POST'])
@limiter.limit('15 per minute')
def pdf():
    # this will be filtered
    user_input = request.form['pdf_form']
    # this won't be filtered and will lead to injection
    user_agent = request.headers.get('User-Agent')
    
    # check for dangerous input
    if any(obj.upper() in user_input.upper() for obj in dangerous_input):
        return redirect(url_for('error', msg = 'Dangerous input detected!'))

    # check if length of input is below certain size
    max_input = 500
    if len(user_input) > max_input or len(user_agent) > max_input:
        return redirect(url_for('error', msg = f'Please keep input smaller than {max_input} characters!'))

    generated_pdf = generate_pdf(user_input, user_agent)
    
    # delete generated PDF after user is done viewing
    @after_this_request
    def remove_file(response):
        try:
            os.remove(generated_pdf)
        except Exception as error:
            app.logger.error(f'Error removing file {generated_pdf}', error)
        return response

    try:
        # display the PDF to the user in browser
        return send_file(generated_pdf)
    except Exception as e:
        return str(e)


def generate_pdf(user_input, user_agent):
    filename = str(uuid.uuid4())
    out_dir = '/var/carnage_uploads'
    out_path = f'{out_dir}/{filename}.pdf'

    if not os.path.exists(out_dir):
        os.makedirs(out_dir)

    options = {
            "enable-local-file-access": None
            }

    pdfkit.from_string(f"""
        <html>
            <head>
                <style>
                    body {{ background-color: white; }}
                    h1 {{ text-align: center; }}
                    div {{ text-align: center; font-size: 20px; }}
                    footer {{ position: absolute; bottom: 0; width: 100%; }}
                </style>
            </head>
        <body>
            <h1>Your PDF, sir</h1>
            <div class=userinput>
                {user_input} </br></br></br>
                Requested by: {request.remote_addr} </br>from {user_agent}
            </div>
        </body>
        </html>
    """, out_path, options=options)

    return out_path

@app.route('/error/<msg>', methods=['GET'])
@limiter.limit('25 per minute')
def error(msg):
    return f"""
    <html>
        <head>
            <title>Error</title>
        </head>

        <div id="app">
            <h3>{escape(msg)}</h3>
        </div>
    </html>
    """

if __name__ == "__main__":
    app.run(host='0.0.0.0')
