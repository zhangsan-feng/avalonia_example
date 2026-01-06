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
	w.ch <- p
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
