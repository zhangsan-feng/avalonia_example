package main

import (
	"avalonia_server/api"
	"avalonia_server/event_bus"
	"avalonia_server/global"
	"github.com/ThreeDotsLabs/watermill/message"
	"github.com/gin-gonic/gin"

	"io"
	"log"
	"os"
	"time"
)

type FileHook struct {
	ch chan []byte
}

func NewFileHook() *FileHook {
	w := &FileHook{ch: make(chan []byte, 1000)}
	go w.processLogs()
	return w
}

func (w *FileHook) Write(p []byte) (n int, err error) {
	data := make([]byte, len(p))
	copy(data, p)
	w.ch <- data
	return len(p), nil
}

func (w *FileHook) processLogs() {
	var currentFile *os.File
	var currentDate string
	defer func() {
		if currentFile != nil {
			_ = currentFile.Close()
		}
	}()
	for p := range w.ch {

		today := time.Now().Format("2006_01_02")
		if currentDate != today {
			if currentFile != nil {
				_ = currentFile.Close()
			}
			filename := "./logs/" + today + ".log"
			f, err := os.OpenFile(filename, os.O_RDWR|os.O_CREATE|os.O_APPEND, 0666)
			if err != nil {
				log.Println("failed to open log file:", err)
				continue
			}
			currentFile = f
			currentDate = today
		}
		_, _ = currentFile.Write(p)
	}
}

func init() {

	go func() {
		defer func() {
			if err := recover(); err != nil {
				log.Println(err)
			}
		}()

		global.EventBus.RegisterEventHandler(event_bus.EventWebSocketMessage, false, func(msg *message.Message) {
			log.Println(string(msg.Payload))
		})
	}()
}

func main() {
	defer func() {
		if err := recover(); err != nil {
			log.Println(err)
		}
	}()
	log.SetFlags(log.LstdFlags | log.Lshortfile)
	if _, osPathErr := os.Stat("./logs"); os.IsNotExist(osPathErr) {
		if osMkdirErr := os.Mkdir("./logs", 0777); osMkdirErr != nil {
			log.Fatalln("os mkdir error")
		}
	}

	//logWriterHandler := io.MultiWriter(os.Stdout)
	logWriterHandler := io.MultiWriter(os.Stdout, NewFileHook())

	log.SetOutput(logWriterHandler)

	gin.SetMode(gin.ReleaseMode)
	go api.CheckWebsocketConn()

	engine := gin.Default()
	api.NewHttpRouter(engine)
	address := "0.0.0.0:34332"
	log.Println("http server start ", address)

	api.InitData()

	if err := engine.Run(address); err != nil {
		log.Println("http server start failed", address)
	}
}

/*
 1a9298f4-7e85-4ff7-99b0-5f540f36433d
 4b27f76c-cb47-4cd3-9e67-e83dd64d31e0
 0e72c6b4-8166-46d8-88dd-5f6ecc88e5a3
 32f644c8-467a-40ac-a911-a4608a98153f
 96ed31b0-19b2-4ffd-abc8-da926608377d
 5bc54ad6-c651-40d2-8b69-211b2a218e68
 89820ded-f3cd-4e07-aa4a-4da5f8735636
 977e8e13-ec4a-4cd6-8e79-5f996bc53868
 9b64e09c-d974-48e4-9e2c-19d806df3578
 48638f48-71a7-40b7-84f9-6359d0f6f88a
 7afc6900-0df5-4930-bdb5-e8d7f3e571a0
 370e58cc-e83d-4736-b200-7f7973526822
 350fd5a5-b43d-4189-b688-5e19596ba994
 5710fd3a-0d35-4f9e-9903-0961da7fa126

*/
