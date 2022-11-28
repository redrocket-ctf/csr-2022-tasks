#[macro_use] extern crate rocket;
use rocket::{State, response::Redirect, form::Form, serde::{Serialize, Deserialize}, fs::{FileServer, relative}, http::{Cookie, CookieJar}, figment::value, log::private::debug};
use rand::prelude::*;
use hex::encode;
use std::{collections::HashMap, sync::{Mutex, Arc}};

struct Data(Arc<Mutex<HashMap<String, String>>>);

#[derive(Debug, Clone, FromForm, Serialize, Deserialize)]
#[serde(crate = "rocket::serde")]
struct OpinionForm {
    #[field(validate = len(0..200))]
    pub value: String,
}

#[post("/set_opinion", data = "<form>")]
async fn set_opinion(jar: &CookieJar<'_>, data: &State<Data>, form: Form<OpinionForm>) {
    let map = &mut *data.0.lock().unwrap();
    let cookie = jar.get("token").map(|c| c.value());
    let value = form.into_inner().value;
    if let Some(token) = cookie {
        if map.contains_key(token) {
            let opinion = map.get_mut(token).unwrap();
            *opinion = value;
        }
    }
}

#[get("/get_opinion")]
async fn get_opinion(jar: &CookieJar<'_>, data: &State<Data>) -> String {
    let map = &*data.0.lock().unwrap();
    let cookie = jar.get("token").map(|c| c.value());
    if let Some(token) = cookie {
        if map.contains_key(token) {
           let value = map.get(token).unwrap().to_string();
           if !value.is_empty() { return value; }
        }
    }
    "once review is set, it will be visible to the admins".to_owned()
}

#[get("/")]
async fn index(jar: &CookieJar<'_>, data: &State<Data>) -> Redirect {
    match jar.get("token").map(|c| c.value()) {
        None => {
            let map = &mut *data.0.lock().unwrap();
            let mut bytes: [u8; 16] = [0;16];
            rand::thread_rng().fill_bytes(&mut bytes);
            let encoded = encode(bytes);
            jar.add(Cookie::new("token", encoded.clone()));
            map.insert(encoded, String::new());
        },
        Some(token) => {
            let map = &mut *data.0.lock().unwrap();
            if !map.contains_key(token) {
                let mut bytes: [u8; 16] = [0;16];
                rand::thread_rng().fill_bytes(&mut bytes);
                let encoded = encode(bytes);
                jar.add(Cookie::new("token", encoded.clone()));
                map.insert(encoded, String::new());
            }
        }
    }
    Redirect::to(uri!("/index.html"))
}

#[get("/debug")]
async fn debug(data: &State<Data>, jar: &CookieJar<'_>) -> String {
    let map = &*data.0.lock().unwrap();
    let token = jar.get("token").map(|c| c.value()).unwrap();
    let keys: Vec<String> = map.clone().into_keys().collect();
    let values: Vec<String> = map.clone().into_values().collect();
    format!("{:?} {:?}", keys, values)
}

#[rocket::main]
async fn main() -> Result<(), rocket::Error> {

    let data = Data(Arc::new(Mutex::new(HashMap::new())));

    let _handler = rocket::build()
        .manage(data)
        .mount("/", routes![index, debug, get_opinion, set_opinion])
        .mount("/", FileServer::from(relative!("static")))
        .launch()
        .await?;
    
    Ok(())
}