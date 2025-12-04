using UnityEngine;

public class ShipEnterExit : MonoBehaviour
{
    public GameObject player;
    public FirstPersonSpaceController playerController;

    public SpaceshipController shipController;

    public Transform playerSeat; // 飞船外玩家位置
    public Transform shipSeat;   // 飞船内座位位置

    private bool inShip = false;

    void Start()
    {
        // 确保飞船控制器初始关闭
        if (shipController != null)
            shipController.ActivateShip(false);
        else
            Debug.LogError("ShipEnterExit: shipController 未赋值！");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!inShip)
                EnterShip();
            else
                ExitShip();
        }
    }

    void EnterShip()
    {
        if (shipController == null || playerController == null || player == null)
        {
            Debug.LogError("EnterShip: 必填引用未赋值！");
            return;
        }

        inShip = true;

        // 禁用玩家控制并隐藏玩家
        playerController.enabled = false;
        player.SetActive(false);

        if (!shipController.enabled)
            shipController.enabled = true;

        // 将飞船摄像机激活
        shipController.ActivateShip(true);

        // 可选：把玩家 Transform 放到飞船座位位置
        if (shipSeat != null)
            player.transform.position = shipSeat.position;

        Debug.Log("进入飞船");
    }

    void ExitShip()
    {
        if (shipController == null || playerController == null || player == null)
        {
            Debug.LogError("ExitShip: 必填引用未赋值！");
            return;
        }

        inShip = false;

        if (shipController.enabled)
            shipController.enabled = false;



        // 停用飞船控制
        shipController.ActivateShip(false);

        // 玩家出现，位置在 playerSeat
        if (playerSeat != null)
            player.transform.position = playerSeat.position;
        player.SetActive(true);

        // 恢复玩家控制
        playerController.enabled = true;

        Debug.Log("离开飞船");
    }
}
