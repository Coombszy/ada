# Ada
A Decent Assistant - RESTful Home assistant project built on several bespoke microservices.

Ada is a project that I started with the intent to learn how to write an application that uses scalable microservices. Each service can be run independently of the rest and utilizes multithreading where possible to improve performance.
Key targets for the project:
- Voice recognition
  - [ ] Wake word/sentence driven
  - [x] Manual button wake
- Text to speech (via Google Cloud TTS API)
  - [x] Retrieve new audio files from Google API
  - [x] Save generated 
  - [x] Reuse saved generated audio
- Triggers and Responses
  - [ ] XML for trigger and response definitions
  - [ ] Load user triggers from XML
  - [ ] Load ada responses (including actions) from XML
  - [ ] Navigate a conversation tree (series of triggers and responses)
  - [ ] Execute actions
- Actions
  - [ ] Load actions from XML
  - [ ] Push actions to Ada queue (driven by user triggers)
  - [ ] Queue client to subscribe to public queue
  - [ ] Execute actions in Ada queue if applicable
- Integrations
  - [ ] IFTT
  - [ ] Android Tasker
  - [ ] Spotify
  - [ ] Smart Mirror
