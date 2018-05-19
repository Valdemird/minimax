﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ManagerScript : MonoBehaviour
{
    public const int VACIO = 0;
    public const int MANZANA = 1;
    public const int CABALLOIA = 2;
    public const int CABALLOJUGADOR = 3;
    public const int MOVIMIENTO_ABAJO_DERECHA = 4;
    public const int MOVIMIENTO_ABAJO_IZQUIERDA = 5;
    public const int MOVIMIENTO_ARRIBA_DERECHA = 6;
    public const int MOVIMIENTO_ARRIBA_IZQUIERDA = 7;
    public const int MOVIMIENTO_DERECHA_ABAJO = 8;
    public const int MOVIMIENTO_DERECHA_ARRIBA = 9;
    public const int MOVIMIENTO_IZQUIERDA_ABAJO = 10;
    public const int MOVIMIENTO_IZQUIERDA_ARRIBA = 11;

    //
    private int turnoActual;
    IAscript iaScript;

    public bool turnoJugador()
    {
        return turnoActual % 2 == 0;
    }

    [SerializeField]
    GameObject casilaOcupada;
    [SerializeField]
    GameObject manazana;
    [SerializeField]
    GameObject CaballoJugador;
    [SerializeField]
    GameObject CaballoIA;
    [SerializeField]
    GameObject CasillaNegra;
    [SerializeField]
    GameObject CasillaBlanca;
    [SerializeField]
    int cantidadItems;
    int[,] representacion;
    int color = 1;
    List<GameObject> jugadasPosibles;

    //Instancias
    GameObject caballoEnjuego;
    GameObject caballoEnJuegoIA;
    ArrayList manzanas;


    //estado del juego

    int manzanasIA;
    int manzanasJugador;


    // Use this for initialization
    void Start()
    {
        iaScript = gameObject.GetComponent<IAscript>();
        representacion = new int[6, 6];
        manzanas = new ArrayList();
        crearTablero();
        inicializacion();
        primerMovimiento();



    }

    public void primerMovimiento()
    {
        DatoPos datos = caballoEnJuegoIA.GetComponent<DatoPos>();
        hacerJugada(datos.PosX, datos.PosY, false);
    }



    public void hacerJugada(int posX, int posY, bool jugador)
    {

        foreach (GameObject jugada in jugadasPosibles) {
            jugada.GetComponent<DatosJugada>().eliminar();
        }

        if (turnoActual % 2 == 0)
        {
            DatoPos datoPos = caballoEnjuego.GetComponent<DatoPos>();
            StartCoroutine(moverCaballo(posX, posY, jugador));

        }
        else
        {
            Vector2 movimiento = iaScript.AlgoritmoMinimax((int[,])representacion.Clone(), manzanasIA, manzanasJugador, posX, posY, 4);
            DatoPos datoPos = caballoEnJuegoIA.GetComponent<DatoPos>();
            StartCoroutine(moverCaballo((int)movimiento.x, (int)movimiento.y, jugador));

        }

        if (representacion[posX, posY] == MANZANA)
        {
            if (jugador)
            {
                manzanasJugador++;
            }
            else
            {
                manzanasIA++;
            }

        }

        turnoActual++;


    }

    private IEnumerator moverCaballo(int posX, int posY, bool jugador)
    {
        GameObject caballo;
        if (jugador)
        {
            caballo = caballoEnjuego;
        }
        else
        {
            caballo = caballoEnJuegoIA;
        }
        Animator anim = caballo.GetComponentInChildren<Animator>();
        anim.SetBool("stop", false);
        NavMeshAgent agent = caballo.GetComponent<NavMeshAgent>();
        Vector3 firstMove = new Vector3(posX, 0, caballo.transform.position.z);
        Vector3 secondMove = new Vector3(posX, 0, posY);
        agent.SetDestination(firstMove);
        yield return new WaitForSeconds(1f);
        agent.SetDestination(secondMove);
        yield return new WaitUntil(() => Vector3.Distance(caballo.transform.position, secondMove) <= 0.2);
        anim.SetBool("stop", true);
        DatoPos datoSiguiente;
        DatoPos datoActual;




        if (jugador)
        {
            datoSiguiente = caballoEnJuegoIA.GetComponent<DatoPos>();
            datoActual = caballoEnjuego.GetComponent<DatoPos>();
            representacion[posX, posY] = CABALLOJUGADOR;
        }
        else
        {
            datoSiguiente = caballoEnjuego.GetComponent<DatoPos>();
            datoActual = caballoEnJuegoIA.GetComponent<DatoPos>();
            representacion[posX, posY] = CABALLOIA;
        }

        representacion[datoActual.PosX, datoActual.PosY] = VACIO;
        datoActual.PosX = posX;
        datoActual.PosY = posY;
        calcularMovimientosPosibles(datoSiguiente.PosX, datoSiguiente.PosY);
        Debug.Log("despues de calcularMovimiento");
        printMatrix();


    }


    void calcularMovimientosPosibles(int posX, int posY)
    {

        for (int i = MOVIMIENTO_ABAJO_DERECHA; i <= MOVIMIENTO_IZQUIERDA_ARRIBA; i++)
        {

            Vector2 posicion = obtenerNuevaPosicion(i, posX, posY);
            if (validarMovimiento((int)posicion.x, (int)posicion.y))
            {

                Vector3 pos = new Vector3(posicion.x, 0, posicion.y);
                GameObject jugada = Instantiate(casilaOcupada, pos, transform.rotation) as GameObject;
                jugada.GetComponent<DatosJugada>().PosX = (int)posicion.x;
                jugada.GetComponent<DatosJugada>().PosY = (int)posicion.y;
                jugada.GetComponent<DatosJugada>().Turno = turnoActual;
                jugadasPosibles.Add(jugada);
            }
        }
    }

    public Vector2 obtenerNuevaPosicion(int movimiento, int posX, int posY)
    {

        switch (movimiento)
        {
            case MOVIMIENTO_ABAJO_DERECHA:
                posY -= 2;
                posX += 1;
                break;
            case MOVIMIENTO_ABAJO_IZQUIERDA:
                posY -= 2;
                posX -= 1;
                break;
            case MOVIMIENTO_ARRIBA_DERECHA:
                posY += 2;
                posX += 1;
                break;
            case MOVIMIENTO_ARRIBA_IZQUIERDA:
                posY += 2;
                posX -= 1;
                break;
            case MOVIMIENTO_DERECHA_ABAJO:
                posY -= 1;
                posX += 2;
                break;
            case MOVIMIENTO_DERECHA_ARRIBA:
                posY += 1;
                posX += 2;
                break;
            case MOVIMIENTO_IZQUIERDA_ABAJO:
                posY -= 1;
                posX -= 2;
                break;
            case MOVIMIENTO_IZQUIERDA_ARRIBA:
                posY += 1;
                posX -= 2;
                break;
        }
        return new Vector2(posX, posY);
    }

    public bool validarMovimiento(int posX, int posY)
    {
        if (posX < 0 || posX > 5 || posY < 0 || posY > 5)
        {
            return false;
        }
        else if (representacion[posX, posY] == CABALLOIA || representacion[posX, posY] == CABALLOJUGADOR)
        {
            return false;
        }
        return true;
    }

    void inicializacion()
    {
        turnoActual = 1;
        manzanasIA = 0;
        manzanasJugador = 0;
        jugadasPosibles = new List<GameObject>();
        createObject(CABALLOIA);
        createObject(CABALLOJUGADOR);


        for (int i = 0; i < cantidadItems; i++)
        {
            createObject(MANZANA);
        }
    }
    void crearTablero()
    {
        for (int i = 0; i < 6; i++)
        {
            if (i % 2 == 0)
            {
                color = 1;
            }
            else
            {
                color = 0;
            }

            for (int j = 0; j < 6; j++)
            {
                if (color == 1)
                {
                    Instantiate(CasillaBlanca, new Vector3(i, 0, j), transform.rotation);
                    color = 0;
                }
                else
                {
                    Instantiate(CasillaNegra, new Vector3(i, 0, j), transform.rotation);
                    color = 1;
                }
            }
        }
    }
    void createObject(int tipo)
    {
        bool escrito = false;
        int posX;
        int posY;
        while (!escrito)
        {
            posX = Random.Range(0, 6);
            posY = Random.Range(0, 6);
            if (representacion[posX, posY] == VACIO)
            {
                escrito = true;
                GameObject elemento = new GameObject();
                Vector3 pos = new Vector3(posX, 0.1f, posY);
                switch (tipo)
                {
                    case MANZANA:
                        GameObject man = Instantiate(manazana, pos, transform.rotation) as GameObject;
                        manzanas.Add(man);
                        break;
                    case CABALLOIA:
                        caballoEnJuegoIA = Instantiate(CaballoIA, pos, transform.rotation) as GameObject;
                        DatoPos datoPosIA = caballoEnJuegoIA.GetComponent<DatoPos>();
                        datoPosIA.PosX = posX;
                        datoPosIA.PosY = posY;
                        break;
                    case CABALLOJUGADOR:

                        caballoEnjuego = Instantiate(CaballoJugador, pos + Vector3.up * 2, transform.rotation) as GameObject;
                        DatoPos datoPos = caballoEnjuego.GetComponent<DatoPos>();
                        datoPos.PosX = posX;
                        datoPos.PosY = posY;
                        break;
                }

                representacion[posX, posY] = tipo;

            }

        }



    }

    public void printMatrix()
    {
        string value = "";
        for (int i = 5; i >= 0; i--)
        {
            for (int j = 0; j < 6; j++)
            {
                value += representacion[j, i] + " ";
            }
            value += "\n";
        }
        Debug.Log(value);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
