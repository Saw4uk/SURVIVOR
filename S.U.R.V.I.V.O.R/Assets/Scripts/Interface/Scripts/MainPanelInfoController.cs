using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class MainPanelInfoController : MonoBehaviour
{
    public PlayerInfoController PlayerInfoController;

    private GroupInfoButtonController ChosenGroupInfoButtonController;
    private GameObject GroupMembersPanel;

    private GroupGameLogic GroupToFirstGroupPanel;
    private GroupGameLogic ChosenGroup;

    private void Awake()
    {
        ChosenGroup = GroupToFirstGroupPanel;// ��� ����� �����, ����� ������ ������� ����� ���������� �� ������ ���� ������, � ������������� ����� ����, � ���� ����� ���.
        GroupToFirstGroupPanel = PlayerInfoController.MainGroup;
        ChosenGroupInfoButtonController = transform.Find("GroupInfoButton").GetComponent<GroupInfoButtonController>();
        ChosenGroupInfoButtonController.ChosenGroup = GroupToFirstGroupPanel;
    }
}
